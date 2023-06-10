using System;
using System.Text;
using FileServer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using MongoConnector;
using MongoConnector.Models;
using NovaanServer.src.ExceptionLayer.CustomExceptions;
using S3Connector;

namespace NovaanServer.src.Content
{
    public class ContentService : IContentService
    {
        private readonly long _videoSizeLimit = 20L * 1024L * 1024L; //20mb
        private readonly string[] _permittedVideoExtensions = { "mp4" };
        // Limits for request body data.
        private static readonly FormOptions _defaultFormOptions = new FormOptions();
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
        public async Task<CulinaryTips> ProcessMultipartRequest(HttpRequest request)
        {
            // Checks if the request's content type is a multipart form data
            if (!IsMultipartContentType(request.ContentType))
            {
                throw new Exception($"Expected a multipart request, but got {request.ContentType}");
            }
            CulinaryTips culinaryTips = new CulinaryTips();
            var filename = string.Empty;
            var streamedFileContent = Array.Empty<byte>();
            var formAccumulator = new KeyValueAccumulator();
            var boundary = GetBoundary(MediaTypeHeaderValue.Parse(request.ContentType), _defaultFormOptions.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, request.Body);
            var section = await reader.ReadNextSectionAsync();
            while (section != null)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);

                if (hasContentDispositionHeader)
                {
                    if (HasFileContentDisposition(contentDisposition))
                    {
                        if (!string.IsNullOrEmpty(contentDisposition.FileName.Value))
                        {
                            streamedFileContent = await ProcessStreamedFile(section, contentDisposition, _permittedVideoExtensions, _videoSizeLimit);
                            culinaryTips.VideoID = await _s3Service.UploadFileAsync(streamedFileContent, section);
                        }
                        else
                        {
                            throw new Exception("Content Disposition has empty filename");
                        }
                    }
                    else if (HasFormDataContentDisposition(contentDisposition))
                    {
                        var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name).Value;
                        var encoding = GetEncoding(section) ?? throw new Exception("Encoding is null");

                        using (var streamReader = new StreamReader(section.Body, encoding, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true))
                        {
                            var value = await streamReader.ReadToEndAsync();
                            if (string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                            {
                                value = string.Empty;
                            }
                            formAccumulator.Append(key!, value);

                            if (formAccumulator.ValueCount > _defaultFormOptions.ValueCountLimit)
                            {
                                throw new InvalidDataException($"Form key count limit {_defaultFormOptions.ValueCountLimit} exceeded.");
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Unknown Content Disposition");
                    }
                }
                else
                {
                    throw new Exception("Invalid Content Disposition");
                }

                section = await reader.ReadNextSectionAsync();
            }

            culinaryTips.AccountID = formAccumulator.GetResults()["accountID"];
            culinaryTips.Title = formAccumulator.GetResults()["title"];
            return culinaryTips;
        }

        private Encoding? GetEncoding(MultipartSection section)
        {
            var hasMediaTypeHeader =
                MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType);


            if (!hasMediaTypeHeader || Encoding.UTF8.Equals(mediaType!.Encoding))
            {
                return Encoding.UTF8;
            }

            return mediaType.Encoding;
        }

        private async Task<byte[]> ProcessStreamedFile(MultipartSection section, ContentDispositionHeaderValue contentDisposition,
    string[] permittedExtensions, long sizeLimit)
        {
            using (var memoryStream = new MemoryStream())
            {
                await section.Body.CopyToAsync(memoryStream);

                // Check if the file is empty or exceeds the size limit.
                if (memoryStream.Length == 0)
                {
                    throw new Exception("File is empty");
                }

                if (memoryStream.Length > sizeLimit)
                {
                    var megabyteSizeLimit = sizeLimit / 1048576;
                    throw new Exception($"The file exceeds {megabyteSizeLimit:N1} MB.");
                }

                var fileName = contentDisposition.FileName.ToString();
                var error = _fileService.IsValidFileExtensionAndSignature(fileName, memoryStream, permittedExtensions);
                if (error != null)
                {
                    throw new Exception(error);
                }

                return memoryStream.ToArray();
            }
        }


        private bool HasFormDataContentDisposition(ContentDispositionHeaderValue? contentDisposition)
        {
            //  For exmaple, Content-Disposition: form-data; name="subdirectory";
            return contentDisposition != null
                && contentDisposition.DispositionType.Equals("form-data")
                && string.IsNullOrEmpty(contentDisposition.FileName.Value)
                && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
        }

        private bool HasFileContentDisposition(ContentDispositionHeaderValue? contentDisposition)
        {
            // For example, Content-Disposition: form-data; name="files"; filename="OnScreenControl_7.58.zip"
            return contentDisposition != null
                && contentDisposition.DispositionType.Equals("form-data")
                && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
                    || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
        }

        private static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

            if (string.IsNullOrWhiteSpace(boundary))
            {
                throw new InvalidDataException("Missing content-type boundary.");
            }

            if (boundary.Length > lengthLimit)
            {
                throw new InvalidDataException(
                    $"Multipart boundary length limit {lengthLimit} exceeded.");
            }

            return boundary;
        }


        private bool IsMultipartContentType(string? contentType)
        {
            return !string.IsNullOrEmpty(contentType)
                  && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}

