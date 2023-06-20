using System.Collections;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using FileServer;
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
        private readonly FileService _fileService;
        private readonly S3Service _s3Service;

        public ContentService(MongoDBService mongoDBService, FileService fileService, S3Service s3Service)
        {
            _mongoService = mongoDBService;
            _fileService = fileService;
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
                     * Indicate that this is an Instruction-related value with following signature:
                     * Instruction_X_Step
                     * Instruction_X_Description
                    */
                    if (property == null)
                    {
                        HandleListSection(obj, fieldName, value);
                    }
                    else
                    {
                        // Normal signature
                        MappingObjectData(obj, property, value);
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
                         * Indicate that this is an Instruction-related value with following signature:
                         * Instruction_X_Image
                        */
                        if (property == null)
                        {
                            HandleListSection(obj, fieldName, fileId);
                        }
                        else
                        {
                            // Normal signature for Video
                            MappingObjectData(obj, property, fileId);
                        }
                    }
                }

                section = await reader.ReadNextSectionAsync();
            }
            return obj;
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

            MappingObjectData(obj, property, value, nestedFieldName, nestedFieldKey);
        }

        // Handle mapping a value into a property in the result object
        private static void MappingObjectData<T>(T? obj, PropertyInfo property, string value)
        {
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
            // Handle default case
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

        // This specially handle situation where user need to upload a list of Instruction
        private static void MappingObjectData<T>(
            T? obj,
            PropertyInfo property,
            string value,
            string nestedField = "",
            int key = 0
        )
        {
            Type propertyType = property.PropertyType;

            // Handle special cases for lists of objects
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var listItemType = propertyType.GetGenericArguments()[0];
                var list = property.GetValue(obj) as IList ??
                    throw new NovaanException(ErrorCodes.SERVER_UNAVAILABLE);
                var listItemProperties = listItemType.GetProperties();

                if (key < list.Count)
                {
                    var listItem = list[key];
                    var subProperty = listItemProperties
                        .FirstOrDefault(p => p.Name == nestedField);

                    if (subProperty == null)
                    {
                        throw new NovaanException(
                            ErrorCodes.CONTENT_FIELD_INVALID,
                            HttpStatusCode.BadRequest
                        );
                    }

                    var convertedValue = Convert.ChangeType(value, subProperty.PropertyType);
                    subProperty.SetValue(listItem, convertedValue);
                    return;
                }

                // When encounter new list item
                if(key == list.Count)
                {
                    var listItem = Activator.CreateInstance(listItemType);
                    var nestedProp = listItemProperties
                        .FirstOrDefault(p => p.Name == nestedField);

                    if (nestedProp == null)
                    {
                        throw new NovaanException(
                            ErrorCodes.CONTENT_FIELD_INVALID,
                            HttpStatusCode.BadRequest
                        );
                    }

                    var convertedValue = Convert.ChangeType(value, nestedProp.PropertyType);
                    nestedProp.SetValue(listItem, convertedValue);
                    list.Add(listItem);
                    return;
                }
            }

            throw new NovaanException(
                ErrorCodes.CONTENT_FIELD_INVALID,
                HttpStatusCode.BadRequest
            );
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

        public async Task UploadTips(CulinaryTip culinaryTips)
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

        

        private async Task<MemoryStream> ProcessStreamContent(FileMultipartSection section)
        {
            var fileStream = section.FileStream;
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
            if(contentExt == ".jpg")
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

