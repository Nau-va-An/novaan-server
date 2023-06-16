using MongoConnector.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NovaanServer.src.Admin.DTOs
{
	public class StatusDTO
	{
		public Status Status { get; set; }
		
		[BsonRepresentation(BsonType.ObjectId)]
		public string UserID { get; set; }

		public SubmissionType SubmissionType { get; set; }	
	}
}

