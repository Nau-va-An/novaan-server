using MongoConnector.Enums;
using MongoConnector.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NovaanServer.src.Search.DTOs
{
    public class RecipeWithUserInfoDTO
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string CreatorId { get; set; }

        public List<User> Author { get; set; } = new();

        public List<Like> Likes { get; set; } = new();

        public Difficulty Difficulty { get; set; } = Difficulty.Easy;

        public int PortionQuantity { get; set; }

        public PortionType PortionType { get; set; } = PortionType.Servings;

        public TimeSpan PrepTime { get; set; }

        public List<Ingredient> Ingredients { get; set; } = new();

        public List<Instruction> Instructions { get; set; } = new();

        public Status Status { get; set; } = Status.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Video { get; set; } = string.Empty;

        public TimeSpan CookTime { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public List<AdminComment> AdminComment { get; set; } = new();

        public List<string> DietPreference { get; set; } = new();

        public List<string> CuisinePreference { get; set; } = new();

        public List<string> MealTypePreference { get; set; } = new();

        public List<string> AllergenPreference { get; set; } = new();
    }

}