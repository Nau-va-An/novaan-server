using System.ComponentModel.DataAnnotations;
using MongoConnector.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoConnector.Models
{
    [BsonIgnoreExtraElements]
    public class CulinaryTip
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string CreatorId { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(55)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MinLength(30)]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MinLength(30)]
        [MaxLength(500)]
        public string Video { get; set; } = string.Empty;

        public Status Status { get; set; } = Status.Pending;

        public double AverageRating { get; set; } = 0;

        public int RatingsCount { get; set; } = 0;

        public int LikesCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string AdminComment { get; set; } = string.Empty;

    }

}