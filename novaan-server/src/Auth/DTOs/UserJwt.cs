using MongoConnector.Models;

namespace NovaanServer.src.Auth.DTOs
{
    public class UserJwt
    {
        public string UserId { get; set; }

        public Role Role { get; set; } = Role.User;
    }
}
