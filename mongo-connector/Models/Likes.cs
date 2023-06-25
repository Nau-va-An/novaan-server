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
        
        public SubmissionType postType { get; set; }
    }
}