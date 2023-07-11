using MongoConnector.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoConnector.Models
{
    public class Like
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string UserId { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string PostId { get; set; }

        // TODO: Add Liked property to check if user like or unlike this post
        public bool Liked { get; set; }

        public SubmissionType postType { get; set; }
    }
}