using MongoConnector;
using MongoConnector.Models;
using NovaanServer.src.Content.DTOs;
using NovaanServer.src.ExceptionLayer.CustomExceptions;
using NovaanServer.src.Image;
using NovaanServer.src.Image.DTOs;
using S3Connector;
using Utils.Hash;

namespace NovaanServer.src.Content
{
    public class RecipeService : IRecipeService
    {
        private readonly S3Service _s3Service;
        private readonly MongoDBService _mongoService;
        public RecipeService(S3Service s3Service, MongoDBService mongoDBService)
        {
            _s3Service = s3Service;
            _mongoService = mongoDBService;
        }

        public async Task<List<string>> UploadImage(List<IFormFile> images)
        {
            List<string> imageIDs = new();
            string id;
            try
            {
                foreach (var image in images)
                {
                    id = _s3Service.UploadFileAsync(image).Result;
                    imageIDs.Add(id);
                }
            }
            catch (TaskCanceledException ex)
            {
                throw new TaskCanceledException(ex.Message);
            }
            return imageIDs;
        }

        public async Task<bool> UploadRecipe(RecipeDTO recipeDTO)
        {
            // Add recipe to database
            var newRecipe = new Recipe
            {
                Title = recipeDTO.Title,
                Instructions = recipeDTO.Instructions,
                Difficulty = recipeDTO.Difficulty,
                PortionType = recipeDTO.PortionType,
                PrepTime = recipeDTO.PrepTime,
                CookTime = recipeDTO.CookTime,
                CreatorId = recipeDTO.CreatorId,
                Status = Status.Pending,
                Images = await UploadImage(recipeDTO.Image)
            };
            
            try
            {
                await _mongoService.Recipes.InsertOneAsync(newRecipe);
            }
            catch
            {
                throw new Exception(ExceptionMessage.SERVER_UNAVAILABLE);
            }
            return true;
        }
    }
}
