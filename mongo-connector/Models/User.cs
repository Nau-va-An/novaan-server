﻿using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoConnector.Models
{
	public class User
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }
		public string DisplayName { get; set; }
		[BsonRepresentation(BsonType.ObjectId)]
		public List<string> Diet { get; set; }
		[BsonRepresentation(BsonType.ObjectId)]
		public List<string> Cuisine { get; set; }
		[BsonRepresentation(BsonType.ObjectId)]
		public List<string> Allergen { get; set; }
		
	}
}

