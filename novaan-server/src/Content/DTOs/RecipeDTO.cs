using MongoConnector.Enums;
using MongoConnector.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NovaanServer.src.Content.DTOs
{
    public class RecipeDTO : Recipe
    {
        public List<User> UsersInfo { get; set; }

        public List<Like> LikeInfo { get; set; }
    }
}
