using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoConnector.Models
{
	public class Account
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string? Id { get; set; }

		public string Username { get; set; }

		public string Email { get; set; }

		public string Password { get; set; }

		public Role Role { get; set; } = Role.User;

		public bool Verified { get; set; } = false;

		public bool EmailSent { get; set; } = false;
	}

	public enum Role
	{
		User,
		Admin
	}
}

