﻿using System.ComponentModel.DataAnnotations;
using System.Net;
using Amazon.S3;
using FileSignatures;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using MongoConnector;
using MongoConnector.Enums;
using MongoConnector.Models;
using MongoDB.Driver;
using NovaanServer.src.Content.DTOs;
using NovaanServer.src.Content.FormHandler;
using NovaanServer.src.Content.Settings;
using NovaanServer.src.ExceptionLayer.CustomExceptions;
using NovaanServer.src.Preference;
using S3Connector;
using Utils.UtilClass;

namespace NovaanServer.src.Content
{
    public class ContentService : IContentService
    {
        private readonly MongoDBService _mongoService;
        private readonly S3Service _s3Service;

        private readonly DelayExecutioner _delayExecutioner = new();

        public ContentService(MongoDBService mongoDBService, S3Service s3Service)
        {
            _mongoService = mongoDBService;
            _s3Service = s3Service;
        }

        // Process multipart request
        public async Task<T> ProcessMultipartRequest<T>(HttpRequest request)
        {
            var boundary = MultipartHandler
                .GetBoundary(MediaTypeHeaderValue.Parse(request.ContentType));
            var reader = new MultipartReader(boundary, request.Body);
            var section = await reader.ReadNextSectionAsync();

            var obj = Activator.CreateInstance<T>() ??
                throw new NovaanException(ErrorCodes.SERVER_UNAVAILABLE);
            var properties = typeof(T).GetProperties();

            while (section != null)
            {
                var disposition = section.GetContentDispositionHeader();
                if (disposition == null || disposition.Name == null)
                {
                    throw new NovaanException(
                        ErrorCodes.CONTENT_DISPOSITION_INVALID,
                        HttpStatusCode.BadRequest
                    );
                }

                if (MultipartHandler.HasFormDataContentDisposition(disposition))
                {
                    // Handle form data
                    var formSection = section.AsFormDataSection() ??
                        throw new NovaanException(
                            ErrorCodes.CONTENT_DISPOSITION_INVALID,
                            HttpStatusCode.BadRequest
                        );

                    var fieldName = formSection.Name;
                    var value = await formSection.GetValueAsync();
                    var property = properties.FirstOrDefault(p => p.Name == fieldName);

                    /*
                     * Indicate that this is an list-related value with following signature:
                     * PropertyName_Index_NestedProperty
                    */
                    if (property == null)
                    {
                        HandleListSection(obj, fieldName, value);
                    }
                    else
                    {
                        // Throw error if property contain video or contain image
                        if (property.Name.ToLower().Contains("video") || property.Name.ToLower().Contains("image"))
                        {
                            throw new NovaanException(
                                ErrorCodes.CONTENT_FIELD_INVALID,
                                HttpStatusCode.BadRequest
                            );
                        }
                        // Normal signature
                        CustomMapper.MappingObjectData(obj, property, value);
                    }
                }

                if (MultipartHandler.HasFileContentDisposition(disposition))
                {
                    // Handle file data
                    var fileSection = section.AsFileSection() ??
                        throw new NovaanException(
                            ErrorCodes.CONTENT_DISPOSITION_INVALID,
                            HttpStatusCode.BadRequest
                        );

                    var fileStream = await ProcessStreamContent(fileSection);

                    var fileId = System.Guid.NewGuid().ToString() +
                            Path.GetExtension(fileSection.FileName);
                    _delayExecutioner.AppendAction(
                        async () => await _s3Service.UploadFileAsync(fileStream, fileId)
                    );

                    var fieldName = fileSection.Name;
                    var property = properties.FirstOrDefault(p => p.Name == fieldName);

                    /*
                     * Indicate that this is an list-related value with following signature:
                     * PropertyName_Index_NestedProperty
                    */
                    if (property == null)
                    {
                        HandleListSection(obj, fieldName, fileId);
                    }
                    else
                    {
                        // Normal signature for Video
                        CustomMapper.MappingObjectData(obj, property, fileId);
                    }
                    // Will close stream when S3 finish uploading
                }

                section = await reader.ReadNextSectionAsync();
            }
            return obj;
        }

        public bool ValidateFileMetadata(FileInformationDTO fileMetadataDTO)
        {
            // Image requirements check
            if (ContentSettings.PermittedImgExtension.Contains(fileMetadataDTO.FileExtension))
            {
                if (fileMetadataDTO.Width < 640 || fileMetadataDTO.Height < 480)
                {
                    throw new NovaanException(ErrorCodes.CONTENT_IMG_RESO_INVALID, HttpStatusCode.BadRequest);
                }

                if (fileMetadataDTO.FileSize > ContentSettings.ImageSizeLimit)
                {
                    throw new NovaanException(ErrorCodes.CONTENT_IMG_SIZE_INVALID, HttpStatusCode.BadRequest);
                }

                return true;
            }

            // Video requirements check
            if (ContentSettings.PermittedVidExtension.Contains(fileMetadataDTO.FileExtension))
            {
                if (fileMetadataDTO.Width / fileMetadataDTO.Height != 16 / 9)
                {
                    throw new NovaanException(ErrorCodes.CONTENT_VID_RESO_INVALID, HttpStatusCode.BadRequest);
                }

                if (fileMetadataDTO.VideoDuration > TimeSpan.FromMinutes(2))
                {
                    throw new NovaanException(ErrorCodes.CONTENT_VID_LEN_INVALID, HttpStatusCode.BadRequest);
                }

                if (fileMetadataDTO.FileSize > ContentSettings.VideoSizeLimit)
                {
                    throw new NovaanException(ErrorCodes.CONTENT_VID_SIZE_INVALID, HttpStatusCode.BadRequest);
                }

                return true;
            }

            throw new NovaanException(ErrorCodes.CONTENT_EXT_INVALID, HttpStatusCode.BadRequest);
        }

        public async Task UploadRecipe(Recipe recipe, string creatorId)
        {
            // Validate recipe object against its model annotation
            var context = new ValidationContext(recipe, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(recipe, context, validationResults, true);
            if (!isValid)
            {
                throw new NovaanException(
                    ErrorCodes.CONTENT_FIELD_INVALID,
                    HttpStatusCode.BadRequest,
                    validationResults.FirstOrDefault().ErrorMessage ?? string.Empty
                );
            }
            if (recipe.Ingredients.Count == 0)
            {
                throw new NovaanException(ErrorCodes.FIELD_REQUIRED, HttpStatusCode.BadRequest);
            }
            if (recipe.Instructions.Count == 0)
            {
                throw new NovaanException(ErrorCodes.FIELD_REQUIRED, HttpStatusCode.BadRequest);
            }
            if (recipe.CookTime.CompareTo(ContentSettings.COOK_MAX) > 0)
            {
                throw new NovaanException(ErrorCodes.CONTENT_COOK_TIME_TOO_LONG, HttpStatusCode.BadRequest);
            }
            if (recipe.PrepTime.CompareTo(ContentSettings.PREP_MAX) > 0)
            {
                throw new NovaanException(ErrorCodes.CONTENT_PREP_TIME_TOO_LONG, HttpStatusCode.BadRequest);
            }

            // Makesure all enum value is valid
            if (!Enum.IsDefined(typeof(Difficulty), recipe.Difficulty))
            {
                throw new NovaanException(ErrorCodes.CONTENT_DIFFICULTY_INVALID, HttpStatusCode.BadRequest);
            }

            if (!Enum.IsDefined(typeof(PortionType), recipe.PortionType))
            {
                throw new NovaanException(ErrorCodes.CONTENT_PORTION_TYPE_INVALID, HttpStatusCode.BadRequest);
            }

            // Check if alleregence preference exist
            bool isPreferencesExist = await ValidatePreferences(recipe.DietPreference, recipe.CuisinePreference, recipe.AllergenPreference, recipe.MealTypePreference);
            if (!isPreferencesExist)
            {
                throw new NovaanException(ErrorCodes.CONTENT_PREFERENCE_NOT_EXIST, HttpStatusCode.BadRequest);
            }

            // Try to push to S3 here
            try
            {
                await _delayExecutioner.Execute();
            }
            catch (AmazonS3Exception e)
            {
                throw new NovaanException(
                    ErrorCodes.S3_UNAVAILABLE,
                    HttpStatusCode.InternalServerError,
                    e.Message
                );
            }
            _delayExecutioner.Cleanup();

            // Set remaining missing fields
            recipe.CreatorId = creatorId;
            recipe.Status = Status.Pending;
            try
            {
                await _mongoService.Recipes.InsertOneAsync(recipe);
            }
            catch
            {
                throw new NovaanException(ErrorCodes.DATABASE_UNAVAILABLE);
            }
        }

        public async Task UploadTips(CulinaryTip culinaryTips, string creatorId)
        {
            // Validate tips and tricks field here
            var context = new ValidationContext(culinaryTips, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(culinaryTips, context, validationResults, true);
            if (!isValid)
            {
                throw new NovaanException(
                    ErrorCodes.CONTENT_FIELD_INVALID,
                    HttpStatusCode.BadRequest,
                    validationResults.FirstOrDefault().ErrorMessage ?? string.Empty
                );
            }

            // Try to push to S3 here
            try
            {
                await _delayExecutioner.Execute();
            }
            catch (AmazonS3Exception e)
            {
                throw new NovaanException(
                    ErrorCodes.S3_UNAVAILABLE,
                    HttpStatusCode.InternalServerError,
                    e.Message
                );
            }
            _delayExecutioner.Cleanup();

            // Set remaning missing field
            culinaryTips.CreatorId = creatorId;
            culinaryTips.Status = Status.Pending;
            try
            {
                await _mongoService.CulinaryTips.InsertOneAsync(culinaryTips);
            }
            catch
            {
                throw new NovaanException(ErrorCodes.DATABASE_UNAVAILABLE);
            }
        }

        public async Task<GetReelDTO> GetPersonalReel(string? userId)
        {
            try
            {
                var recipes = (await _mongoService.Recipes
                    .FindAsync(r => r.Status == Status.Approved))
                    .ToList();
                var culinaryTips = (await _mongoService.CulinaryTips
                    .FindAsync(c => c.Status == Status.Approved))
                    .ToList();
                return new GetReelDTO
                {
                    Recipes = recipes,
                    CulinaryTips = culinaryTips
                };
            }
            catch
            {
                throw new NovaanException(ErrorCodes.DATABASE_UNAVAILABLE);
            }
        }

        private static void HandleListSection<T>(T? obj, string fieldName, string value)
        {
            var splitValues = fieldName.Split("_");

            if (splitValues.Length > 3)
            {
                throw new NovaanException(ErrorCodes.CONTENT_FIELD_INVALID, HttpStatusCode.BadRequest);
            }
            // splitValues[0] should always be "Instruction"
            var properties = typeof(T).GetProperties();
            var property = properties.FirstOrDefault(p => p.Name == splitValues[0]) ??
                throw new NovaanException(ErrorCodes.FIELD_NOT_FOUND);

            bool canParse = int.TryParse(splitValues[1], out var nestedFieldKey);
            if (!canParse)
            {
                throw new NovaanException(ErrorCodes.CONTENT_FIELD_INVALID, HttpStatusCode.BadRequest);
            }

            // Handle case that have nested property
            if (splitValues.Count() == 3)
            {
                var nestedFieldName = splitValues[2];

                CustomMapper.MappingObjectData(obj, property, value, nestedFieldName, nestedFieldKey);
            }
            else
            {
                CustomMapper.MappingObjectData(obj, property, value, null, nestedFieldKey);
            }
        }

        private async Task<MemoryStream> ProcessStreamContent(FileMultipartSection section)
        {
            var fileStream = section.FileStream ??
                throw new NovaanException(ErrorCodes.SERVER_UNAVAILABLE);
            var memStream = new MemoryStream();
            await fileStream.CopyToAsync(memStream);

            // Check if the file is empty
            if (fileStream == null || memStream == null || memStream.Length == 0)
            {
                throw new Exception("File is empty");
            }

            var extension = Path.GetExtension(section.FileName);
            if (extension == null)
            {
                throw new NovaanException(ErrorCodes.CONTENT_EXT_INVALID, HttpStatusCode.BadRequest);
            }

            var isImage = ContentSettings.PermittedImgExtension.Contains(extension);
            var isVideo = !isImage && ContentSettings.PermittedVidExtension.Contains(extension);
            if (!isImage && !isVideo)
            {
                throw new NovaanException(ErrorCodes.CONTENT_EXT_INVALID, HttpStatusCode.BadRequest);
            }

            if (isImage)
            {
                if (fileStream.Length > ContentSettings.ImageSizeLimit)
                {
                    throw new NovaanException(ErrorCodes.CONTENT_IMG_SIZE_INVALID, HttpStatusCode.BadRequest);
                }
                ValidateFileExtensionAndSignature(
                    extension,
                    memStream,
                    ContentSettings.PermittedImgExtension
                );
            }

            if (isVideo)
            {
                if (fileStream.Length > ContentSettings.VideoSizeLimit)
                {
                    throw new NovaanException(ErrorCodes.CONTENT_VID_SIZE_INVALID, HttpStatusCode.BadRequest);
                }
                ValidateFileExtensionAndSignature(
                    extension,
                    memStream,
                    ContentSettings.PermittedVidExtension
                );
            }

            return memStream;
        }

        private static void ValidateFileExtensionAndSignature(
            string extension,
            MemoryStream data,
            string[] permittedExtensions
        )
        {
            var inspector = new FileFormatInspector();
            var fileFormat = inspector.DetermineFileFormat(data);

            // JPG and JPEG is the same
            var contentExt = "." + fileFormat?.Extension;
            if (contentExt == ".jpg" && extension == ".jpeg")
            {
                contentExt = ".jpeg";
            }

            if (
                fileFormat == null ||
                extension != contentExt
            )
            {
                throw new NovaanException(ErrorCodes.CONTENT_EXT_INVALID, HttpStatusCode.BadRequest);
            }
        }

        public async Task<bool> ValidatePreferences(HashSet<string>? dietPreference, HashSet<string>? cuisinesPreference, HashSet<string>? allergensPreference, HashSet<string>? mealTypesPreference)
        {
            // If any of those hash set is not empty and have any element not in the database, return false
            if (dietPreference != null && dietPreference.Count != 0)
            {
                var diet = (await _mongoService.Diets.FindAsync(_ => true)).ToList();
                var dietIds = diet.Select(d => d.Id).ToHashSet();
                if (dietPreference.Any(d => !dietIds.Contains(d)))
                {
                    return false;
                }
            }

            if (cuisinesPreference != null && cuisinesPreference.Count != 0)
            {
                var cuisine = (await _mongoService.Cuisines.FindAsync(_ => true)).ToList();
                var cuisineIds = cuisine.Select(c => c.Id).ToHashSet();
                if (cuisinesPreference.Any(c => !cuisineIds.Contains(c)))
                {
                    return false;
                }
            }

            if (allergensPreference != null && allergensPreference.Count != 0)
            {
                var allergen = (await _mongoService.Allergens.FindAsync(_ => true)).ToList();
                var allergenIds = allergen.Select(a => a.Id).ToHashSet();
                if (allergensPreference.Any(a => !allergenIds.Contains(a)))
                {
                    return false;
                }
            }

            if (mealTypesPreference != null && mealTypesPreference.Count != 0)
            {
                var mealType = (await _mongoService.MealTypes.FindAsync(_ => true)).ToList();
                var mealTypeIds = mealType.Select(m => m.Id).ToHashSet();
                if (mealTypesPreference.Any(m => !mealTypeIds.Contains(m)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

