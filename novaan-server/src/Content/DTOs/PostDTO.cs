using System;
using MongoConnector.Models;

namespace NovaanServer.src.Content.DTOs
{
	public class PostDTO
	{
		public List<Recipe> RecipeList { get; set; }
		public List<CulinaryTip> CulinaryTipList { get; set; }
	}
}

