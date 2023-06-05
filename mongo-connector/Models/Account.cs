using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NovaanServer.Models
{
	public class Account
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string? Id { get; set; }

		public string Email { get; set; }

		public string Password { get; set; }

		public Role Role { get; set; }
	}

	public enum Role
	{
		User,
		Admin
	}
}

