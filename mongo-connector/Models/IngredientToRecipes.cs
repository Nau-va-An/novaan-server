using MongoDB.Bson.Serialization.Attributes;

namespace mongo_connector.Models
{
    public class IngredientToRecipes
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }

        public string Ingredient { get; set; } 

        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public List<string> RecipeIds { get; set; }
    }
    
}