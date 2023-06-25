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
        public string AccountID { get; set; }

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

    }
}

