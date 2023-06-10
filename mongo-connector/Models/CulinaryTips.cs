using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoConnector.Models
{
    public class CulinaryTips
    {
        [BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        // Title
        public string? Title { get; set; }
        // VideoURL
        public string? VideoID { get; set; }
        // AccountID
        public string? AccountID { get; set; }
    }
    
}