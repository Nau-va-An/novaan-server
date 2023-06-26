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
        [BsonRepresentation(BsonType.ObjectId)]
        public string FollowerId { get; set; }

        [Required]
        [BsonRepresentation(BsonType.ObjectId)]
        public string FollowingId { get; set; }
    }
}