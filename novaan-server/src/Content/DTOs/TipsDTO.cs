using MongoConnector.Models;

namespace NovaanServer.src.Content.DTOs
{
    public class TipsDTO : CulinaryTip
    {
        public List<User> UsersInfo { get; set; } 

        public List<Like> LikeInfo { get; set; } 
    }
}