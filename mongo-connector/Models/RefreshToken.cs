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

        public string UserId { get; set; }

        public string CurrentJwtId { get; set; }

        public bool IsRevoked { get; set; } = false;

        // Support multi session (signin on multiple device)
        public List<string> ValidTokenFamily { get; set; }

        public List<string> RevokeTokenFamily { get; set; }

        public DateTime AddedDate { get; set; }

        public DateTime ExpiryDate { get; set; }
    }
}
