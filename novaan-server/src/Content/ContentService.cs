using System.Collections;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using FileSignatures;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using MongoConnector;
using MongoConnector.Enums;
using MongoConnector.Models;
using MongoDB.Driver;
using Newtonsoft.Json;
using NovaanServer.src.Content.DTOs;
using NovaanServer.src.Content.FormHandler;
using NovaanServer.src.ExceptionLayer.CustomExceptions;
using S3Connector;
using Utils.Json;

namespace NovaanServer.src.Content
{
    public class ContentService : IContentService
    {
        private readonly long _videoSizeLimit = 20L * 1024L * 1024L; // 20MB
        private readonly long _imageSizeLimit = 5L * 1024L * 1024L; // 5MB

        private readonly string[] _permittedVideoExtensions = { ".mp4" };
        private readonly string[] _permittedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        private readonly MongoDBService _mongoService;
        private readonly S3Service _s3Service;

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

                    using (var fileStream = await ProcessStreamContent(fileSection))
                    {
                        var fileId = await _s3Service.UploadFileAsync(
                            fileStream,
                            Path.GetExtension(fileSection.FileName)
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
                    }
                }

                section = await reader.ReadNextSectionAsync();
            }
            return obj;
        }

        public bool ValidateFileMetadata(FileInformationDTO fileMetadataDTO)
        {
            // Check if file extension is in permitted extensions
            if (!_permittedVideoExtensions.Contains(fileMetadataDTO.FileExtension) &&
                !_permittedImageExtensions.Contains(fileMetadataDTO.FileExtension))
            {
                throw new NovaanException(ErrorCodes.CONTENT_EXT_INVALID, HttpStatusCode.BadRequest);
            }

            // Image requirements check
            if (_permittedImageExtensions.Contains(fileMetadataDTO.FileExtension))
            {
                if (fileMetadataDTO.Width < 640 || fileMetadataDTO.Height < 480)
                {
                    throw new NovaanException(ErrorCodes.CONTENT_IMG_RESO_INVALID, HttpStatusCode.BadRequest);
                }

                if (fileMetadataDTO.FileSize > _imageSizeLimit)
                {
                    throw new NovaanException(ErrorCodes.CONTENT_IMG_SIZE_INVALID, HttpStatusCode.BadRequest);
                }
            }

            // Video requirements check
            if (_permittedVideoExtensions.Contains(fileMetadataDTO.FileExtension))
            {
                if (fileMetadataDTO.Width / fileMetadataDTO.Height != 16 / 9)
                {
                    throw new NovaanException(ErrorCodes.CONTENT_VID_RESO_INVALID, HttpStatusCode.BadRequest);
                }

                if (fileMetadataDTO.VideoDuration > TimeSpan.FromMinutes(2))
                {
                    throw new NovaanException(ErrorCodes.CONTENT_VID_LEN_INVALID, HttpStatusCode.BadRequest);
                }

                if (fileMetadataDTO.FileSize > _videoSizeLimit)
                {
                    throw new NovaanException(ErrorCodes.CONTENT_VID_SIZE_INVALID, HttpStatusCode.BadRequest);
                }
            }

            return true;
        }

        public async Task UploadRecipe(Recipe recipe)
        {
            // Validate recipe fields here
            try
            {
                await _mongoService.Recipes.InsertOneAsync(recipe);
            }
            catch
            {
                throw new NovaanException(ErrorCodes.DATABASE_UNAVAILABLE);
            }
        }

        public async Task UploadTips(CulinaryTip culinaryTips)
        {
            // Validate tips and tricks field here
            try
            {
                await _mongoService.CulinaryTips.InsertOneAsync(culinaryTips);
            }
            catch
            {
                throw new NovaanException(ErrorCodes.DATABASE_UNAVAILABLE);
            }
        }

        public PostDTO GetPosts()
        {
            try
            {
                var recipes = _mongoService.Recipes.Find(r => r.Status == Status.Approved).ToList();
                var culinaryTips = _mongoService.CulinaryTips.Find(c => c.Status == Status.Approved).ToList();
                return new PostDTO
                {
                    RecipeList = recipes,
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
            if (splitValues.Length != 3)
            {
                throw new NovaanException(
                    ErrorCodes.CONTENT_FIELD_INVALID,
                    HttpStatusCode.BadRequest
                );
            }

            // splitValues[0] should always be "Instruction"
            var properties = typeof(T).GetProperties();
            var property = properties.FirstOrDefault(p => p.Name == splitValues[0]) ??
                throw new NovaanException(ErrorCodes.SERVER_UNAVAILABLE);


            bool canParse = int.TryParse(splitValues[1], out var nestedFieldKey);
            if (!canParse)
            {
                throw new NovaanException(ErrorCodes.CONTENT_FIELD_INVALID, HttpStatusCode.BadRequest);
            }
            var nestedFieldName = splitValues[2];

            CustomMapper.MappingObjectData(obj, property, value, nestedFieldName, nestedFieldKey);
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

            var isImage = _permittedImageExtensions.Contains(extension);
            var isVideo = isImage ? false : _permittedVideoExtensions.Contains(extension);
            if (!isImage && !isVideo)
            {
                throw new NovaanException(ErrorCodes.CONTENT_EXT_INVALID, HttpStatusCode.BadRequest);
            }

            if (isImage)
            {
                if (fileStream.Length > _imageSizeLimit)
                {
                    throw new NovaanException(ErrorCodes.CONTENT_IMG_SIZE_INVALID, HttpStatusCode.BadRequest);
                }
                ValidateFileExtensionAndSignature(extension, memStream, _permittedImageExtensions);
            }

            if (isVideo)
            {
                if (fileStream.Length > _videoSizeLimit)
                {
                    throw new NovaanException(ErrorCodes.CONTENT_VID_SIZE_INVALID, HttpStatusCode.BadRequest);
                }
                ValidateFileExtensionAndSignature(extension, memStream, _permittedVideoExtensions);
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
            if (contentExt == ".jpg")
            {
                contentExt = ".jpeg";
            }

            if (
                fileFormat == null ||
                !permittedExtensions.Contains(contentExt) ||
                extension != contentExt
            )
            {
                throw new NovaanException(ErrorCodes.CONTENT_EXT_INVALID, HttpStatusCode.BadRequest);
            }
        }
    }
}

