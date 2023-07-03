using System;
using MongoConnector.Models;

namespace NovaanServer.src.Content.DTOs
{
    public class GetReelDTO
    {
        public List<Recipe> Recipes { get; set; } = new List<Recipe>();

        public List<CulinaryTip> CulinaryTips { get; set; } = new List<CulinaryTip>();
    }
}

