using MongoConnector.Models;

namespace NovaanServer.src.Content.DTOs
{
    public class GetCommentWithUserInfoDTO : Comments
    {
        public List<User> UserInfo { get; set; }
    }
}