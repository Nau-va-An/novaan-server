using MongoConnector.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoConnector.Models
{
    public class CulinaryTip
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Title { get; set; }

        public string Video { get; set; }

        public string AccountID { get; set; }

        public Status Status { get; set; } = Status.Pending;
    }

}