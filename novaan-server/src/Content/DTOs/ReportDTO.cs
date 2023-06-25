using System.ComponentModel.DataAnnotations;

namespace NovaanServer.src.Content.DTOs
{
    public class ReportDTO{
        [Required]
        public string Reason { get; set; } = string.Empty;
    }
}