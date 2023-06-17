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

        public List<SubmissionsDTO> GetSubmissions(Status status)
        {
            var submissions = new List<SubmissionsDTO>();
            var foundRecipes = _mongoService.Recipes.Find(x => x.Status == status).ToList();
            var foundTips = _mongoService.CulinaryTips.Find(x => x.Status == status).ToList();
            submissions.Add(new SubmissionsDTO { Recipes = foundRecipes, CulinaryTips = foundTips });
            return submissions;
        }

        public void UpdateStatus<T>(StatusDTO statusDTO)
        {
            var statusEnum = (Status)statusDTO.Status;
            var submissionType = statusDTO.SubmissionType.ToString().ToLower();
            var collection = _mongoService.GetCollection<T>(submissionType);
            var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(statusDTO.UserID));
            if (collection.Find(filter).FirstOrDefault() == null)
            {
                throw new Exception("Recipe not found");
            }
            var update = Builders<T>.Update.Set("Status", statusEnum);
            collection.UpdateOne(filter, update);
        }
    }
}

