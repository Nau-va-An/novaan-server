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

        public string DisplayName { get; set; } = string.Empty;

        public string Avatar { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public HashSet<string> Diet { get; set; } = new();

        [BsonRepresentation(BsonType.ObjectId)]
        public HashSet<string> Cuisine { get; set; } = new();

        [BsonRepresentation(BsonType.ObjectId)]
        public HashSet<string> Allergen { get; set; } = new();

        public int FollowerCount { get; set; }

        public int FollowingCount { get; set; }
      
        public List<SavedPost> SavedPost { get; set; } = new();
    }
}

