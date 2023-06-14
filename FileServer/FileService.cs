using System;
using System.Text;
using FileSignatures;

namespace FileServer
{
    public class FileService
    {
        public void ValidateFileExtensionAndSignature(string fileName, Stream data, string[] permittedExtensions)
        {
            if (string.IsNullOrEmpty(fileName) || data == null || data.Length == 0)
            {
                throw new ArgumentException("File name or data is invalid.");
            }

            var inspector = new FileFormatInspector();
            var fileFormat = inspector.DetermineFileFormat(data);

            if (fileFormat == null)
            {
                throw new ArgumentException("File format is invalid.");
            }

            string fileExtension = Path.GetExtension(fileName);
            // Remove the dot
            fileExtension = fileExtension.Substring(1);

            if (!permittedExtensions.Contains(fileFormat.Extension) || fileFormat.Extension != fileExtension)
            {
                throw new ArgumentException("File extension is invalid.");
            }
        }

    }
}

