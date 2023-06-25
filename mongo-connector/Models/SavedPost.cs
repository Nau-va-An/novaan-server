using System;
using MongoConnector.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoConnector.Models
{
    public class SavedPost
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string PostId { get; set; }

        public SubmissionType PostType { get; set; } 
    }
}