using System;
using MongoConnector.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NovaanServer.src.Profile.DTOs
{
	public class ProfileResDTO
	{
		public string Username { get; set; }

		[BsonRepresentation(BsonType.ObjectId)]
		public string UserId { get; set; }

		public bool IsFollowing { get; set; }

		public int FollowersCount { get; set; }

		public int FollowingCount { get; set; }

		public string Avatar { get; set; }

		public List<Recipe> RecipeList { get; set; }

	}
}

