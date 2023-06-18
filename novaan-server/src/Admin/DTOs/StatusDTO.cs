using MongoConnector.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NovaanServer.src.Admin.DTOs
{
    public class StatusDTO
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string PostID { get; set; } = string.Empty;

        public string UserID { get; set; } = string.Empty;

        public Status Status { get; set; }

        public SubmissionType SubmissionType { get; set; }
    }
}

