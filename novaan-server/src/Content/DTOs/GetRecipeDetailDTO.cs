using MongoConnector.Enums;
using MongoConnector.Models;

namespace NovaanServer.src.Content.DTOs
{
    public class GetRecipeDetailDTO{
        public string Id { get; set; }

        public string CreatorId { get; set; }

        public string CreatorName { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Video { get; set; } = string.Empty;

        public int Difficulty { get; set; } 

        public int PortionQuantity { get; set; }

        public int PortionType { get; set; }

        public TimeSpan PrepTime { get; set; }

        public TimeSpan CookTime { get; set; }

        public List<Instruction> Instructions { get; set; } = new List<Instruction>();

        public List<Ingredient> Ingredients { get; set; } = new List<Ingredient>();

        public Status Status { get; set; } = Status.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public AdminComment? AdminComment { get; set; }

        public bool IsLiked { get; set; }

        public bool IsSaved { get; set; }
    }
}