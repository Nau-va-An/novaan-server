using System.ComponentModel.DataAnnotations;

namespace NovaanServer.src.Content.DTOs
{
    public class FileInformationDTO
    {
        [Required]
        public long FileSize { get; set; }

        [Required]
        public int Width { get; set; }

        [Required]
        public int Height { get; set; }

        [Required]
        public string? FileExtension { get; set; }

        public TimeSpan? VideoDuration { get; set; }
    }

}

