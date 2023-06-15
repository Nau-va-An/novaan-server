using System;
using MongoConnector.Models;

namespace NovaanServer.src.Admin.DTOs
{
	public class SubmissionsDTO
	{
		public List<Recipe> Recipes { get; set; }
		public List<CulinaryTips> CulinaryTips { get; set; }
	}
}

