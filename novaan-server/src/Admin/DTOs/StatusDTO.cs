using MongoConnector.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NovaanServer.src.Admin.DTOs
{
    public class UpdateStatusDTO
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string PostId { get; set; } 

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } 

        public Status Status { get; set; }

        public string AdminComment { get; set; } = string.Empty;
    }
}

