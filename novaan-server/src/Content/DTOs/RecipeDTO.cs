using MongoConnector.Models;

namespace NovaanServer.src.Content.DTOs
{
    public class RecipeDTO : Recipe
    {
        public List<User> UsersInfo { get; set; } = new List<User>();

        public List<Like> LikeInfo { get; set; } = new List<Like>();
    }
}
