using MongoConnector.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NovaanServer.src.Profile.DTOs
{
    public class UserDTO
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        public string ProfilePicture { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public HashSet<string> Diet { get; set; } = new HashSet<string>();

        [BsonRepresentation(BsonType.ObjectId)]
        public HashSet<string> Cuisine { get; set; } = new HashSet<string>();

        [BsonRepresentation(BsonType.ObjectId)]
        public HashSet<string> Allergen { get; set; } = new HashSet<string>();

        public int FollowerCount { get; set; }

        public int FollowingCount { get; set; }

        public List<SavedPost> SavedPost { get; set; } = new List<SavedPost>();

        public List<Followership> Followerships { get; set; }
    }

}