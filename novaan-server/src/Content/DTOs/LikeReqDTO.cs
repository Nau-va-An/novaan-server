using System.ComponentModel.DataAnnotations;
using MongoConnector.Enums;

namespace NovaanServer.src.Content.DTOs
{
    public class LikeReqDTO{

        [Required]
        public SubmissionType PostType { get; set; }
        [Required]
        public bool Liked { get; set; }
    }
}