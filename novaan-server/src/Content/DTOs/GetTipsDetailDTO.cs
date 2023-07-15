
using MongoConnector.Enums;
using MongoConnector.Models;

namespace NovaanServer.src.Content.DTOs
{
    public class GetTipsDetailDTO
    {
        public string Id { get; set; } = string.Empty;

        public string CreatorId { get; set; } = string.Empty;

        public string CreatorName { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Video { get; set; } = string.Empty;

        public Status Status { get; set; } = Status.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public AdminComment? AdminComment { get; set; } 

        public bool IsLiked { get; set; }

        public bool IsSaved { get; set; }
    }
}