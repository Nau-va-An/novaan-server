using System;
using MongoConnector.Models;

namespace NovaanServer.src.Content.DTOs
{
    public class PostDTO
    {
        public List<Recipe> RecipeList { get; set; } = new List<Recipe>();

        public List<CulinaryTip> CulinaryTips { get; set; } = new List<CulinaryTip>();
    }
}

