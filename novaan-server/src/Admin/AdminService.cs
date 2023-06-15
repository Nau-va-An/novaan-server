using System;
using MongoConnector;
using MongoConnector.Enums;
using MongoConnector.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using NovaanServer.src.Admin.DTOs;

namespace NovaanServer.src.Admin
{
    public class AdminService : IAdminService
    {
        private readonly MongoDBService _mongoService;
        public AdminService(MongoDBService mongoService)
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

        public Task UpdateStatus<T>(string submissionType,string id, int status)
        {
            var statusEnum = (Status)status;
            try
            {
                // Get collection of object
                var collection = _mongoService.GetCollection<T>(submissionType);
                // Get object by id, note that id in database is object id
                var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
                if(collection.Find(filter).FirstOrDefault() == null)
                {
                    throw new Exception("Recipe not found");
                }
                // Update status
                var update = Builders<T>.Update.Set("Status", statusEnum);
                // Update object
                collection.UpdateOne(filter, update);
                return Task.CompletedTask;
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}

