using MongoConnector.Models;

namespace NovaanServer.src.Auth.DTOs
{
    public class UserJwt
    {
        public string Email { get; set; }

        public Role Role { get; set; }

    }
}
