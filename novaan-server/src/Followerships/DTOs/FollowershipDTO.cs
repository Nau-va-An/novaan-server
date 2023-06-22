using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NovaanServer.src.Followerships.DTOs
{
    public class FollowershipDTO
    {
        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string UserID { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string FollowedUserId { get; set; } = string.Empty;
    }
}

