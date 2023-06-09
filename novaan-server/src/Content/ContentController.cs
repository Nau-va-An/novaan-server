using System;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using NovaanServer.src.Filter;
using S3Connector;

namespace NovaanServer.src.Content
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContentController : ControllerBase
    {
        private readonly IContentService _contentService;
        // Limits for request body data.
        private static readonly FormOptions _defaultFormOptions = new FormOptions();
        private readonly string[] _permittedVideoExtensions = { ".mp4", ".gif" };
        // For more file signatures, see the File Signatures Database (https://www.filesignatures.net/)
        // and the official specifications for the file types you wish to add.

        private static readonly Dictionary<string, List<byte[]>> _fileSignature = new Dictionary<string, List<byte[]>>
        {
            { ".mp4", GenerateMp4Signature()},
            { ".gif", new List<byte[]> { new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61} } },
            { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".jpeg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                }
            },
            { ".jpg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 },
                }
            },
            { ".zip", new List<byte[]>
                {
                    new byte[] { 0x50, 0x4B, 0x03, 0x04 },
                    new byte[] { 0x50, 0x4B, 0x4C, 0x49, 0x54, 0x45 },
                    new byte[] { 0x50, 0x4B, 0x53, 0x70, 0x58 },
                    new byte[] { 0x50, 0x4B, 0x05, 0x06 },
                    new byte[] { 0x50, 0x4B, 0x07, 0x08 },
                    new byte[] { 0x57, 0x69, 0x6E, 0x5A, 0x69, 0x70 },
                }
            },
        };

        private readonly long _videoSizeLimit = 20L * 1024L * 1024L; //20mb
        public ContentController(IContentService contentService)
        {
            _contentService = contentService;
        }

        [HttpPost("upload/culinary-tips")]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> UploadCulinaryTips()
        {
            // Checks if the request's content type is a multipart form data
            if (!IsMultipartContentType(Request.ContentType))
            {
                return BadRequest($"Expected a multipart request, but got {Request.ContentType}");
            }

            var filename = string.Empty;
            var streamedFileContent = Array.Empty<byte>();

            var boundary = GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType), _defaultFormOptions.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            var section = await reader.ReadNextSectionAsync();
            while (section != null)
            {
                var hasContentDispositionHeader =
                    ContentDispositionHeaderValue.TryParse(
                        section.ContentDisposition, out var contentDisposition);

                if (hasContentDispositionHeader)
                {
                    if (HasFileContentDisposition(contentDisposition))
                    {
                        if (contentDisposition != null)
                        {
                            if (!string.IsNullOrEmpty(contentDisposition.FileName.Value))
                            {
                                streamedFileContent = await ProcessStreamedFile(section, contentDisposition, _permittedVideoExtensions, _videoSizeLimit);
                            }
                        }
                        else
                        {
                            throw new Exception("Content Disposition is null");
                        }
                    }
                }
                // Get content type
                var contentType = section.ContentType;
                var fileSection = section.AsFileSection();
                if (fileSection != null)
                {
                    // Don't trust the file name sent by the client. To display
                    // the file name, HTML-encode the value.
                    var fileName = WebUtility.HtmlEncode(fileSection.FileName);
                    var uploadService = new S3Service();
                    // Upload file stream to S3
                    await uploadService.UploadFileAsync(streamedFileContent, fileName, contentType);
                }
                else
                {
                    throw new Exception("File section is null");
                }

                section = await reader.ReadNextSectionAsync();
            }
            return Ok();
        }


        private async Task<byte[]> ProcessStreamedFile(MultipartSection section, ContentDispositionHeaderValue contentDisposition,
            string[] permittedExtensions, long sizeLimit)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await section.Body.CopyToAsync(memoryStream);

                    // Check if the file is empty or exceeds the size limit.
                    if (memoryStream.Length == 0)
                    {
                        //Return error 
                        throw new Exception("File is empty");

                    }
                    else if (memoryStream.Length > sizeLimit)
                    {
                        var megabyteSizeLimit = sizeLimit / 1048576;
                        throw new Exception($"The file exceeds {megabyteSizeLimit:N1} MB.");
                    }
                    else if (!IsValidFileExtensionAndSignature(
                        contentDisposition.FileName.ToString(), memoryStream,
                        permittedExtensions))
                    {
                        throw new Exception("The file type isn't permitted or the file's signature doesn't match the file's extension.");
                    }
                    else
                    {
                        return memoryStream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private bool IsValidFileExtensionAndSignature(string fileName, Stream data, string[] permittedExtensions)
        {
            if (string.IsNullOrEmpty(fileName) || data == null || data.Length == 0)
            {
                return false;
            }

            var ext = Path.GetExtension(fileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
            {
                return false;
            }

            data.Position = 0;

            using (var reader = new BinaryReader(data))
            {
                var signatures = _fileSignature[ext];
                IEnumerable<byte>? headerBytes = null;
                switch (ext)
                {
                    case ".mp4":
                        headerBytes = reader.ReadBytes(12).Skip(4).Take(8);
                        break;
                    default:
                        headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));
                        break;
                }
                return signatures.Any(signature =>
                    headerBytes.Take(signature.Length).SequenceEqual(signature));
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

        public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
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

        // Generate Mp4 signature for all sub-type
        private static List<byte[]> GenerateMp4Signature()
        {
            List<string> mp4Subtypes = new List<string>
        {
            "avc1", "iso2", "isom", "mmp4", "mp41", "mp42", "mp71", "msnv",
            "ndas", "ndsc", "ndsh", "ndsm", "ndsp", "ndss", "ndxc", "ndxh",
            "ndxm", "ndxp", "ndxs"
        };
            List<byte[]> byteArraylist = new List<byte[]>();
            foreach (var subtype in mp4Subtypes)
            {
                var subtypeBytes = Encoding.ASCII.GetBytes(subtype);
                var signature = new byte[] { 0x66, 0x74, 0x79, 0x70 };
                var signatureBytes = signature.Concat(subtypeBytes).ToArray();
                byteArraylist.Add(signatureBytes);
            }
            return byteArraylist;
        }
    }
}

