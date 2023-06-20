using System;
using Microsoft.Net.Http.Headers;

namespace NovaanServer.src.Content.FormHandler
{
	public class MultipartHandler
	{
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
    }
}

