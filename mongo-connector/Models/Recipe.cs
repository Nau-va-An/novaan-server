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

        public string Title { get; set; } = string.Empty;

        public string Video { get; set; } = string.Empty;

        public Difficulty Difficulty { get; set; } = Difficulty.Easy;

        public int PortionQuantity { get; set; }

        public PortionType PortionType { get; set; } = PortionType.Servings;

        public TimeSpan PrepTime { get; set; }

        public TimeSpan CookTime { get; set; }

        public List<Instruction> Instructions { get; set; } = new List<Instruction>();

        [BsonRepresentation(BsonType.ObjectId)]
        public string CreatorId { get; set; }

        public List<Ingredient> Ingredients { get; set; } = new List<Ingredient>();

        public Status Status { get; set; } = Status.Pending;
    }
}
