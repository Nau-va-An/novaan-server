using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoConnector.Models
{
    public class RefreshToken
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string CurrentJwtId { get; set; }

        [BsonRepresentation(BsonType.Boolean)]
        public bool IsRevoked { get; set; }

        [BsonRepresentation(BsonType.Array)]
        public List<string> TokenFamily { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime AddedDate { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime ExpiryDate { get; set; }
    }
}
