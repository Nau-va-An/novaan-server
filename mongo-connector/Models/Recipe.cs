using System.ComponentModel.DataAnnotations;
using MongoConnector.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoConnector.Models
{
    public class Recipe
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
        public string Video { get; set; } = string.Empty;

        [Required]
        public Difficulty Difficulty { get; set; } = Difficulty.Easy;

        [Required]
        public int PortionQuantity { get; set; }

        [Required]
        public PortionType PortionType { get; set; } = PortionType.Servings;

        [Required]
        public TimeSpan PrepTime { get; set; }

        [Required]
        public TimeSpan CookTime { get; set; }

        [Required]
        public List<Instruction> Instructions { get; set; } = new List<Instruction>();

        [Required]
        public List<Ingredient> Ingredients { get; set; } = new List<Ingredient>();

        public double AverageRating { get; set;} = 0;

        public int RatingsCount { get; set; } = 0;

        public int LikesCount { get; set; } = 0;

        public Status Status { get; set; } = Status.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}
