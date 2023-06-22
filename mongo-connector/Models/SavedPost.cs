using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoConnector.Models
{
    public class SavedPost
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string PostID { get; set; }

        public string PostType { get; set; } = string.Empty;
    }
}

