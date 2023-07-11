using System.ComponentModel.DataAnnotations;
using MongoConnector.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoConnector.Models
{
    public class Likes
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string UserId { get; set; }
        
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string PostId { get; set; }

        // TODO: Add Liked property to check if user like or unlike this post
        [Range(0, 1)]
        public int Liked { get; set; }
        
        public SubmissionType PostType { get; set; }
    }
}