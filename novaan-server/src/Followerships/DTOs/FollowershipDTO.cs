using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NovaanServer.src.Followerships.DTOs
{
    public class FollowershipDTO
    {
        public int? Start { get; set; }
        public int? Limit { get; set; }
        public string UserName { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;

        public string Avatar { get; set; } = string.Empty;

        public bool IsFollowed { get; set; }
    }   
}

