using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoConnector.Models
{
	public class MealType
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Title { get; set; }
	}
}

