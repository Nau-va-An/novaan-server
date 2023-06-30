using System.ComponentModel.DataAnnotations;
using MongoConnector.Enums;

namespace NovaanServer.src.Content.DTOs
{
    public class ReportDTO{
        public SubmissionType PostType { get; set; }
        [Required]
        public string Reason { get; set; } = string.Empty;
    }
}