using System;
using FileSignatures;

namespace FileServer
{
    public class FileService
    {
        public string? IsValidFileExtensionAndSignature(string fileName, Stream data, string[] permittedExtensions)
        {
            if (string.IsNullOrEmpty(fileName) || data == null || data.Length == 0)
            {
                return "Invalid file data or file name.";
            }

            var inspector = new FileFormatInspector();
            var fileFormat = inspector.DetermineFileFormat(data);

            if (fileFormat == null)
            {
                return "Unable to determine file format.";
            }

            string fileExtension = Path.GetExtension(fileName);

            if (permittedExtensions.Contains(fileFormat.Extension) || fileFormat.Extension == fileExtension)
            {
                return null; // No error, valid file extension and signature
            }

            return "Invalid file extension or signature.";
        }

    }
}

