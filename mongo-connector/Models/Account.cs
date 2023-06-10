using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoConnector.Models
{
	public class Account
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }

		[BsonRepresentation(BsonType.String)]
		public string Username { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string Email { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string Password { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Role Role { get; set; } = Role.User;

        [BsonRepresentation(BsonType.Boolean)]
        public bool Verified { get; set; } = false;

        [BsonRepresentation(BsonType.Boolean)]
        public bool EmailSent { get; set; } = false;
	}

	public enum Role
	{
		User,
		Admin
	}
}

