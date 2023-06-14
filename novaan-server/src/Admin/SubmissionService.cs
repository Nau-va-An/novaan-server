using System;
using MongoConnector;
using MongoConnector.Enums;
using MongoConnector.Models;
using MongoDB.Driver;
using NovaanServer.src.Admin.DTOs;

namespace NovaanServer.src.Admin
{
    public class SubmissionService : ISubmissionService
    {
        private readonly MongoDBService _mongoService;
        public SubmissionService(MongoDBService mongoService)
        {
            _mongoService = mongoService;
        }

        public List<SubmissionsDTO> GetSubmissions(int status)
        {
            var statusEnum = (Status)status;
			var submissions = new List<SubmissionsDTO>();			
			try
            {
				// Get all recipes with status
                var recipes = _mongoService.Recipes.Find(x => x.Status == statusEnum).ToList();
				// Get all culinary tips with status
				var culinaryTips = _mongoService.CulinaryTips.Find(x => x.Status == statusEnum).ToList();
				// Add to DTO
				submissions.Add(new SubmissionsDTO { Recipes = recipes, CulinaryTips = culinaryTips });
				return submissions;
            }
            catch (System.Exception)
			{
				throw;
			}
        }
    }
}

