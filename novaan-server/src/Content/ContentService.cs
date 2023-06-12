using System;
using System.Text;
using FileServer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using MongoConnector;
using MongoConnector.Models;
using NovaanServer.src.Content.DTOs;
using NovaanServer.src.ExceptionLayer.CustomExceptions;
using S3Connector;

namespace NovaanServer.src.Content
{
    public class ContentService : IContentService
    {
        private readonly long _videoSizeLimit = 20L * 1024L * 1024L; // 20mb
        // _imageSizeLimit is 5,242,880 bytes.
        private readonly long _imageSizeLimit = 5L * 1024L * 1024L; // 5mb

        private const int VALUE_COUNT_LIMIT = 1024;
        private const int MULTIPART_BOUNDARY_LENGTH_LIMIT = 128;
        private readonly string[] _permittedVideoExtensions = { "mp4" };
        private readonly string[] _permittedImageExtensions = { "jpg", "jpeg", "png" };
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
        public async Task<Dictionary<string, object>> ProcessMultipartRequest(HttpRequest request)
        {
            // Checks if the request's content type is a multipart form data
            if (!IsMultipartContentType(request.ContentType))
            {
                throw new Exception($"Expected a multipart request, but got {request.ContentType}");
            }
            // Create an array to store file ID
            var imageIDs = new List<string>();
            // variable to store videoID
            string videoID = "";
            // Create a dictionary to store form data key-value pairs
            var formData = new Dictionary<string, object>();
            var boundary = GetBoundary(MediaTypeHeaderValue.Parse(request.ContentType));
            var reader = new MultipartReader(boundary, request.Body);
            var section = await reader.ReadNextSectionAsync();
            while (section != null)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);

                if (contentDisposition == null)
                {
                    throw new Exception("Content Disposition is null");
                }
                if (!hasContentDispositionHeader)
                {
                    throw new Exception("Invalid Content Disposition");
                }
                if (HasFileContentDisposition(contentDisposition))
                {
                    var streamedFileContent = await ProcessStreamedFile(section, contentDisposition, _permittedVideoExtensions, _videoSizeLimit);
                    // Upload file to S3
                    var fileID = await _s3Service.UploadFileAsync(streamedFileContent, section);
                    // Close stream
                    streamedFileContent.Close();
                    // Check if fileId contain string in permittedImageExtensions
                    if (_permittedImageExtensions.Contains(fileID.Split('.')[1]))
                    {
                        imageIDs.Add(fileID);
                        formData.Add("Images", imageIDs);
                    }
                    else
                    {
                        videoID = fileID;
                        formData.Add("Video", videoID);
                    }
                }
                else if (HasFormDataContentDisposition(contentDisposition))
                {
                    var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name).Value;
                    var encoding = GetEncoding(section);

                    if (key == null)
                    {
                        throw new Exception("Field name is null");
                    }

                    using (var streamReader = new StreamReader(section.Body, encoding, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true))
                    {
                        var value = await streamReader.ReadToEndAsync();
                        if (string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                        {
                            value = string.Empty;
                        }
                        formData.Add(key, value);

                        if (formData.Count > VALUE_COUNT_LIMIT)
                        {
                            throw new InvalidDataException($"Form key count limit {VALUE_COUNT_LIMIT} exceeded.");
                        }
                    }
                }
                else
                {
                    throw new Exception("Unknown Content Disposition");
                }
                section = await reader.ReadNextSectionAsync();
            }

            return formData;
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

    }
}

