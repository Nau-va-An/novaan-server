using System;
using MongoConnector.Models;

namespace NovaanServer.src.Admin.DTOs
{
    public class SubmissionsDTO
    {
        public List<Recipe> Recipes { get; set; } = new List<Recipe>();
        
        public List<CulinaryTip> CulinaryTips { get; set; } = new List<CulinaryTip>();
    }
}

