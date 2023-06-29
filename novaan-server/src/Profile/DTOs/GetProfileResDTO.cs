using System;
using MongoConnector.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NovaanServer.src.Profile.DTOs
{
    public class GetProfileResDTO
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Username { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        public bool IsFollowing { get; set; }

        public int FollowersCount { get; set; }

        public int FollowingCount { get; set; }
        public List<Recipe> Recipes { get; set; } = new List<Recipe>();

        public string Avatar { get; set; } = string.Empty;

        public List<Followership> Followerships { get; set; }


    }
}

