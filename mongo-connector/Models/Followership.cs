using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoConnector.Models
{
    public class Followership
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } 

        [Required]
        public string FollowerId { get; set; } = string.Empty;

        [Required]
        public string FollowingId { get; set; } = string.Empty;
    }
}