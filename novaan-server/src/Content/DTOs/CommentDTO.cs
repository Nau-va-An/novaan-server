using System.ComponentModel.DataAnnotations;
using MongoConnector.Enums;

namespace NovaanServer.src.Content.DTOs
{
    public class CommentDTO
    {
        public string? Comment { get; set; }

        public IFormFile? Image { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Field value must be between 1 and 5.")]
        public int Rating { get; set; }

        [Required]
        public SubmissionType PostType { get; set; }

        public Status CommentStatus { get; set; } = Status.Approved;
    }

}