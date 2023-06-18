using System;
using MongoConnector;
using MongoConnector.Enums;
using MongoConnector.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using NovaanServer.src.Admin.DTOs;
using NovaanServer.src.ExceptionLayer.CustomExceptions;

namespace NovaanServer.src.Admin
{
    public class AdminService : IAdminService
    {
        private readonly MongoDBService _mongoService;
        public AdminService(MongoDBService mongoService)
        {
            _mongoService = mongoService;
        }

        public SubmissionsDTO GetSubmissions(Status status)
        {
            try
            {
                var foundRecipes = _mongoService.Recipes.Find(x => x.Status == status).ToList();
                var foundTips = _mongoService.CulinaryTips.Find(x => x.Status == status).ToList();

                return new SubmissionsDTO { Recipes = foundRecipes, CulinaryTips = foundTips };
            }
            catch
            {
                throw new Exception(ExceptionMessage.DATABASE_UNAVAILABLE);
            }
        }

        public void UpdateStatus<T>(StatusDTO statusDTO, string collectionName)
        {
            var collection = _mongoService.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(statusDTO.PostId));

            // Check if the submission is available
            var foundSubmission = collection.Find(filter).FirstOrDefault() ??
                throw new NotFoundException($"Submission with id: {statusDTO.PostId} not found");

            // Try to update the submission with inputted status
            var update = Builders<T>.Update.Set("Status", statusDTO.Status);
            try
            {
                var result = collection.UpdateOne(filter, update);
                if(!result.IsAcknowledged)
                {
                    throw new Exception();
                }
            }catch
            {
                throw new Exception(ExceptionMessage.DATABASE_UNAVAILABLE);
            }
        }
    }
}

