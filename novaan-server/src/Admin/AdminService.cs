using System.Net;
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

        public SubmissionsDTO GetSubmissions(List<Status> status)
        {
            try
            {
                var recipesFilter = Builders<Recipe>.Filter.In("Status", status);
                var culinaryTipsFilter = Builders<CulinaryTip>.Filter.In("Status", status);

                var recipesResult = _mongoService.Recipes.Find(recipesFilter).ToList();
                var culinaryTipsResult = _mongoService.CulinaryTips.Find(culinaryTipsFilter).ToList();

                return new SubmissionsDTO
                {
                    Recipes = recipesResult,
                    CulinaryTips = culinaryTipsResult
                };
            }
            catch
            {
                throw new NovaanException(ErrorCodes.DATABASE_UNAVAILABLE);
            }
        }

        public void UpdateStatus<T>(UpdateStatusDTO statusDTO, string collectionName)
        {
            var collection = _mongoService.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(statusDTO.PostId));

            // Check if the submission is available
            var foundSubmission = collection.Find(filter).FirstOrDefault() ??
                throw new NovaanException(ErrorCodes.ADMIN_SUBMISSION_NOT_FOUND, HttpStatusCode.NotFound);

            // Try to update the submission with inputted status
            var update = Builders<T>.Update.Set("Status", statusDTO.Status);
            try
            {
                var result = collection.UpdateOne(filter, update);
                if (!result.IsAcknowledged)
                {
                    throw new Exception();
                }
            }
            catch
            {
                throw new NovaanException(ErrorCodes.DATABASE_UNAVAILABLE);
            }
        }
    }
}

