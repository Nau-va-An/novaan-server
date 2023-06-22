using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoConnector.Models
{
    public class Followership
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } 
        public string FollowerId { get; set; } = string.Empty;

        public string FollowingId { get; set; } = string.Empty;
    }
}