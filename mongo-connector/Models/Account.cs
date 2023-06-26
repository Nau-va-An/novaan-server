using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoConnector.Models
{
    public class Account
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public Role Role { get; set; } = Role.User;

        public bool Verified { get; set; } = false;

        public bool EmailSent { get; set; } = false;

        // Google ID
        public string? GoogleId { get; set; }

    }

    public enum Role
    {
        User,
        Admin
    }
}

