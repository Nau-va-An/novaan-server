using System;
namespace NovaanServer.src.Content.DTOs
{
    public class FileInformationDTO
    {
        public long FileSize { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string? FileExtension { get; set; }
        public TimeSpan VideoDuration { get; set; }
    }

}

