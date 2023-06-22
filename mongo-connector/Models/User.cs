using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoConnector.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string AccountId { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string ProfilePicture { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> Diet { get; set; } = new List<string>();

        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> Cuisine { get; set; } = new List<string>();

        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> Allergen { get; set; } = new List<string>();

        public int FollowerCount { get; set; }

        public int FollowingCount { get; set; }

    }
}

