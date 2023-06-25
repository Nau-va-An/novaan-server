using MongoConnector.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoConnector.Models
{
    public class Comments
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string UserId { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string PostId { get; set; }

        public SubmissionType postType { get; set; }
       
        public string? Comment { get; set; } 

        public string? Image { get; set; }

        public int? Rating { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}