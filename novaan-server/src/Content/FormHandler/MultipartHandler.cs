using System;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Microsoft.Net.Http.Headers;

namespace NovaanServer.src.Content.FormHandler
{
    public class MultipartHandler
    {
        private const int MULTIPART_BOUNDARY_LENGTH_LIMIT = 128;

        public static bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            // e.g. Content-Disposition: form-data; name="subdirectory";
            return contentDisposition.DispositionType.Equals("form-data")
                && string.IsNullOrEmpty(contentDisposition.FileName.Value)
                && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
        }

        public static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            // e.g. Content-Disposition: form-data; name="files"; filename="OnScreenControl_7.58.zip"
            return contentDisposition.DispositionType.Equals("form-data")
                && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
                    || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
        }

        public static string GetBoundary(MediaTypeHeaderValue contentType)
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
    }
}

