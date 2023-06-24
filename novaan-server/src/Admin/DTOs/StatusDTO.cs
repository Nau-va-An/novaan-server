using MongoConnector.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NovaanServer.src.Admin.DTOs
{
    public class StatusDTO
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string PostId { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public Status Status { get; set; }
    }
}

