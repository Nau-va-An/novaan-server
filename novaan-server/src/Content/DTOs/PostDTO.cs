using System;
using MongoConnector.Models;

namespace NovaanServer.src.Content.DTOs
{
	public class PostDTO
	{
		public List<Recipe> RecipeList { get; set; } = new List<Recipe>();

		public List<CulinaryTip> CulinaryTipList { get; set; } = new List<CulinaryTip>();
	}
}

