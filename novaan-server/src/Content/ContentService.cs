﻿using System.Collections;
using System.Net;
using System.Reflection;
using System.Text;
using FileServer;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using MongoConnector;
using MongoConnector.Enums;
using MongoConnector.Models;
using MongoDB.Driver;
using Newtonsoft.Json;
using NovaanServer.src.Content.DTOs;
using NovaanServer.src.ExceptionLayer.CustomExceptions;
using S3Connector;
using Utils.Json;

namespace NovaanServer.src.Content
{
    // TODO: WARNING: Need to do a full refactor on this service
    public class ContentService : IContentService
    {
        private const int MULTIPART_BOUNDARY_LENGTH_LIMIT = 128;

        private readonly long _videoSizeLimit = 20L * 1024L * 1024L; // 20mb
        private readonly long _imageSizeLimit = 5L * 1024L * 1024L; // 5mb

        private readonly string[] _permittedVideoExtensions = { "mp4" };
        private readonly string[] _permittedImageExtensions = { "jpg", "jpeg", "png", "webp" };

        private readonly MongoDBService _mongoService;
        private readonly FileService _fileService;
        private readonly S3Service _s3Service;

        public ContentService(MongoDBService mongoDBService, FileService fileService, S3Service s3Service)
        {
            _mongoService = mongoDBService;
            _fileService = fileService;
            _s3Service = s3Service;
        }

        public async Task AddCulinaryTips(CulinaryTip culinaryTips)
        {
            try
            {
                await _mongoService.CulinaryTips.InsertOneAsync(culinaryTips);
            }
            catch
            {
                throw new NovaanException(ErrorCodes.DATABASE_UNAVAILABLE);
            }
        }

        // Process multipart request
        public async Task<T> ProcessMultipartRequest<T>(HttpRequest request)
        {
            if (!IsMultipartContentType(request.ContentType))
            {
                throw new NovaanException(ErrorCodes.CONTENT_CONTENT_TYPE_INVALID, HttpStatusCode.BadRequest);
            }

            var boundary = GetBoundary(MediaTypeHeaderValue.Parse(request.ContentType));
            var reader = new MultipartReader(boundary, request.Body);
            var section = await reader.ReadNextSectionAsync();

            var obj = Activator.CreateInstance<T>();
            var properties = typeof(T).GetProperties();

            while (section != null)
            {
                var contentDisposition = ContentDispositionHeaderValue.Parse(section.ContentDisposition);
                if (contentDisposition == null || contentDisposition.Name == null)
                {
                    throw new NovaanException(ErrorCodes.CONTENT_DISPOSITION_INVALID, HttpStatusCode.BadRequest);
                }
                var fieldName = HeaderUtilities.RemoveQuotes(contentDisposition.Name).Value.ToLowerInvariant();
                var property = properties.FirstOrDefault(p => p.Name.ToLowerInvariant() == fieldName);

                if (HasFileContentDisposition(contentDisposition))
                {
                    if (contentDisposition.FileName.Value == null)
                    {
                        throw new NovaanException(ErrorCodes.CONTENT_FILENAME_INVALID, HttpStatusCode.BadRequest);
                    }
                    var isImage = fieldName.Contains("image");
                    var permittedExtensions = isImage ? _permittedImageExtensions : _permittedVideoExtensions;
                    var sizeLimit = isImage ? _imageSizeLimit : _videoSizeLimit;

                    using (var streamedFileContent = await ProcessStreamedFile(section, contentDisposition, permittedExtensions, sizeLimit))
                    {
                        var fileID = await _s3Service.UploadFileAsync(streamedFileContent, section);

                        if (property != null)
                        {
                            MappingObjectData(obj, property, fileID);
                        }
                        else
                        {
                            // For example, object T have a property named "Instruction" with data type is List<Instruction>
                            // Instruction object have a property named "Image" with data type is string
                            // The fieldName that front-end passed to back-end will be "Instruction_Image_1"
                            var splitValues = fieldName.Split('_');
                            if (splitValues.Length != 3)
                            {
                                throw new NovaanException(ErrorCodes.CONTENT_FIELD_INVALID, HttpStatusCode.BadRequest);
                            }

                            var fieldNameSplit = splitValues[0];
                            var subFieldName = splitValues[1];
                            var key = splitValues[2];

                            property = properties.FirstOrDefault(p => p.Name.ToLowerInvariant() == fieldNameSplit);
                            if (property == null)
                            {
                                throw new NovaanException(ErrorCodes.CONTENT_FIELD_INVALID, HttpStatusCode.BadRequest);
                            }

                            MappingObjectData(obj, property, fileID, subFieldName, int.Parse(key));
                        }
                    }
                }
                else if (HasFormDataContentDisposition(contentDisposition))
                {
                    var encoding = GetEncoding(section);
                    using (var streamReader = new StreamReader(section.Body, encoding, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true))
                    {
                        var value = await streamReader.ReadToEndAsync();
                        if (string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                        {
                            value = string.Empty;
                        }

                        if (property != null)
                        {
                            MappingObjectData(obj, property, value);
                        }
                    }
                }
                else
                {
                    throw new NovaanException(ErrorCodes.CONTENT_DISPOSITION_INVALID, HttpStatusCode.BadRequest);
                }

                section = await reader.ReadNextSectionAsync();
            }

            return obj;
        }


        private static void MappingObjectData<T>(T? obj, PropertyInfo? property, string value, string subFieldName = "", int key = 0)
        {
            if (property == null)
            {
                throw new Exception("Unknown Property");
            }

            Type propertyType = property.PropertyType;

            // Handle special cases for enums
            if (propertyType.IsEnum)
            {
                var enumValue = Enum.Parse(propertyType, value);
                property.SetValue(obj, enumValue);
            }

            // Handle special cases for TimeSpan
            else if (propertyType == typeof(TimeSpan))
            {
                if (TimeSpan.TryParse(value, out var timeSpan))
                {
                    property.SetValue(obj, timeSpan);
                }
            }

            // Handle special cases for lists of objects
            else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var listItemType = propertyType.GetGenericArguments()[0];
                var list = property.GetValue(obj) as IList;
                var listItemProperties = listItemType.GetProperties();

                if (key != 0 && key <= list.Count)
                {
                    var listItem = list[key - 1];
                    var subProperty = listItemProperties.FirstOrDefault(p => p.Name.ToLower() == subFieldName.ToLower());

                    if (subProperty != null)
                    {
                        var convertedValue = Convert.ChangeType(value, subProperty.PropertyType);
                        subProperty.SetValue(listItem, convertedValue);
                    }
                }
                else
                {
                    var listValue = JsonConvert.DeserializeObject(value, propertyType);
                    property.SetValue(obj, listValue);
                }
            }
            // Default case: convert value to the property type
            else
            {
                var convertedValue = value as IConvertible;
                if (convertedValue != null)
                {
                    var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
                    var convertedObject = convertedValue.ToType(targetType, null);
                    property.SetValue(obj, convertedObject);
                }
            }
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
            try
            {
                await _mongoService.Recipes.InsertOneAsync(recipe);
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

        private static bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            //  For exmaple, Content-Disposition: form-data; name="subdirectory";
            return contentDisposition.DispositionType.Equals("form-data")
                && string.IsNullOrEmpty(contentDisposition.FileName.Value)
                && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
        }

        private static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            // For example, Content-Disposition: form-data; name="files"; filename="OnScreenControl_7.58.zip"
            return contentDisposition.DispositionType.Equals("form-data")
                && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
                    || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
        }

        private static bool IsMultipartContentType(string? contentType)
        {
            return !string.IsNullOrEmpty(contentType)
                  && contentType.Contains("multipart/", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetBoundary(MediaTypeHeaderValue contentType)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

            if (string.IsNullOrWhiteSpace(boundary))
            {
                throw new InvalidDataException("Missing content-type boundary.");
            }

            if (boundary.Length > MULTIPART_BOUNDARY_LENGTH_LIMIT)
            {
                throw new InvalidDataException(
                    $"Multipart boundary length limit {MULTIPART_BOUNDARY_LENGTH_LIMIT} exceeded.");
            }

            return boundary;
        }

        private async Task<MemoryStream> ProcessStreamedFile(
            MultipartSection section,
            ContentDispositionHeaderValue contentDisposition,
            string[] permittedExtensions,
            long sizeLimit
        )
        {
            var memoryStream = new MemoryStream();
            await section.Body.CopyToAsync(memoryStream);

            // Check if the file is empty or exceeds the size limit.
            if (memoryStream.Length == 0)
            {
                throw new Exception("File is empty");
            }

            if (memoryStream.Length > sizeLimit)
            {
                var megabyteSizeLimit = sizeLimit / 1024 / 1024;
                throw new Exception($"The file exceeds {megabyteSizeLimit:N1} MB.");
            }

            var fileName = contentDisposition.FileName.ToString();

            // Validate file extension and signature
            _fileService.ValidateFileExtensionAndSignature(fileName, memoryStream, permittedExtensions);

            return memoryStream;
        }

        private static Encoding GetEncoding(MultipartSection section)
        {
            var hasMediaTypeHeader =
                MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType);

            if (!hasMediaTypeHeader ||
                mediaType == null ||
                mediaType.Encoding == null)
            {
                return Encoding.UTF8;
            }

            return mediaType.Encoding;
        }
    }
}

