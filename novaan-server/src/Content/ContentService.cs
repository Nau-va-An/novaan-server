﻿using System.Collections;
using System.Reflection;
using System.Text;
using FileServer;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using MongoConnector;
using MongoConnector.Models;
using Newtonsoft.Json;
using NovaanServer.src.Content.DTOs;
using NovaanServer.src.ExceptionLayer.CustomExceptions;
using S3Connector;
using Utils.Json;

namespace NovaanServer.src.Content
{
    public class ContentService : IContentService
    {
        private readonly long _videoSizeLimit = 20L * 1024L * 1024L; // 20mb
        // _imageSizeLimit is 5,242,880 bytes.
        private readonly long _imageSizeLimit = 5L * 1024L * 1024L; // 5mb
        private const int MULTIPART_BOUNDARY_LENGTH_LIMIT = 128;
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
        public async Task AddCulinaryTips(CulinaryTips culinaryTips)
        {
            try
            {
                await _mongoService.CulinaryTips.InsertOneAsync(culinaryTips);
            }
            catch (Exception ex)
            {
                throw new Exception(ExceptionMessage.SERVER_UNAVAILABLE);
            }
        }

        // Process multipart request
        public async Task<T> ProcessMultipartRequest<T>(HttpRequest request)
        {
            if (!IsMultipartContentType(request.ContentType))
            {
                throw new Exception($"Expected a multipart request, but got {request.ContentType}");
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
                    throw new Exception("Invalid Content Disposition");
                }
                var fieldName = HeaderUtilities.RemoveQuotes(contentDisposition.Name).Value.ToLowerInvariant();
                var property = properties.FirstOrDefault(p => p.Name.ToLowerInvariant() == fieldName);

                if (HasFileContentDisposition(contentDisposition))
                {
                    if (contentDisposition.FileName.Value == null)
                    {
                        throw new Exception("Invalid File Name");
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
                                throw new Exception("Invalid Field Name");
                            }

                            var fieldNameSplit = splitValues[0];
                            var subFieldName = splitValues[1];
                            var key = splitValues[2];

                            property = properties.FirstOrDefault(p => p.Name.ToLowerInvariant() == fieldNameSplit);
                            if (property == null)
                            {
                                throw new Exception("Unknown Field Name");
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
                    throw new Exception("Unknown Content Disposition");
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

        private Encoding GetEncoding(MultipartSection section)
        {
            var hasMediaTypeHeader =
                MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType);

            if (!hasMediaTypeHeader || Encoding.UTF8.Equals(mediaType.Encoding))
            {
                return Encoding.UTF8;
            }

            return mediaType.Encoding;
        }

        private async Task<MemoryStream> ProcessStreamedFile(MultipartSection section, ContentDispositionHeaderValue contentDisposition,
    string[] permittedExtensions, long sizeLimit)
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

            //validate file extension and signature
            _fileService.ValidateFileExtensionAndSignature(fileName, memoryStream, permittedExtensions);

            return memoryStream;
        }


        private bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            //  For exmaple, Content-Disposition: form-data; name="subdirectory";
            return contentDisposition.DispositionType.Equals("form-data")
                && string.IsNullOrEmpty(contentDisposition.FileName.Value)
                && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
        }

        private bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            // For example, Content-Disposition: form-data; name="files"; filename="OnScreenControl_7.58.zip"
            return contentDisposition.DispositionType.Equals("form-data")
                && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
                    || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
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


        private bool IsMultipartContentType(string? contentType)
        {
            return !string.IsNullOrEmpty(contentType)
                  && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public Task ValidateFileMetadata(FileInformationDTO fileMetadataDTO)
        {
            // Check if file extension is in permitted extensions
            if (!_permittedVideoExtensions.Contains(fileMetadataDTO.FileExtension) || !_permittedImageExtensions.Contains(fileMetadataDTO.FileExtension))
            {
                throw new Exception("Invalid file extension");
            }

            // Check if file size is null
            if (fileMetadataDTO.FileSize == null)
            {
                throw new Exception("File size is null");
            }

            // If file is image
            if (_permittedImageExtensions.Contains(fileMetadataDTO.FileExtension))
            {
                // Check if size is at least 640 x 480
                if (fileMetadataDTO.Width < 640 || fileMetadataDTO.Height < 480)
                {
                    throw new Exception("Image size is too small");
                }

                if (fileMetadataDTO.FileSize > _imageSizeLimit)
                {
                    throw new Exception("File is too large");
                }

            }

            // If file is video
            if (_permittedVideoExtensions.Contains(fileMetadataDTO.FileExtension))
            {
                // Check if it has ratio is 16:9
                if (fileMetadataDTO.Width / fileMetadataDTO.Height != 16 / 9)
                {
                    throw new Exception("Video ratio is not 16:9");
                }

                // Check if video duration is less than 2 minutes
                if (fileMetadataDTO.VideoDuration > TimeSpan.FromMinutes(2))
                {
                    throw new Exception("Video duration is too long");
                }

                if (fileMetadataDTO.FileSize > _videoSizeLimit)
                {
                    throw new Exception("Video size is too large");
                }
            }
            return Task.CompletedTask;
        }

        public async Task UploadRecipe(Recipes recipe)
        {
            try
            {
                _mongoService.Recipes.InsertOne(recipe);
            }
            catch (System.Exception)
            {

                throw new Exception(ExceptionMessage.SERVER_UNAVAILABLE);
            }
        }
    }
}

