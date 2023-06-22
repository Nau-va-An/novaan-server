using System;
using System.Net;
using MongoConnector;
using MongoConnector.Enums;
using MongoConnector.Models;
using MongoDB.Driver;
using NovaanServer.src.Common.DTOs;
using NovaanServer.src.Common.Utils;
using NovaanServer.src.ExceptionLayer.CustomExceptions;
using NovaanServer.src.Profile.DTOs;

namespace NovaanServer.src.Profile
{
    public class ProfileService : IProfileService
    {
        private readonly MongoDBService _mongoDBService;

        public ProfileService(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        public async Task<ProfileRESDTO> GetProfile(string currentUserId, string userID)
        {
            User currentUser = (await _mongoDBService.Users.FindAsync(u => u.Id == currentUserId)).FirstOrDefault() ?? throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            User user = (await _mongoDBService.Users.FindAsync(u => u.Id == userID)).FirstOrDefault() ?? throw new NovaanException(ErrorCodes.PROFILE_USER_NOT_FOUND, HttpStatusCode.NotFound);
            // check if current user is following the user
            bool isFollowing = (await _mongoDBService.Followerships.FindAsync(f => f.FollowerId == currentUserId && f.FollowingId == userID)).Any();
            var recipeList = (await _mongoDBService.Recipes.FindAsync(r => r.CreatorId == user.AccountID)).ToList().Take(10);
            var profile = new ProfileRESDTO
            {
                UserName = user.DisplayName,
                UserID = user.Id,
                Avatar = user.ProfilePicture,
                RecipeList = recipeList.ToList(),
                FollowersCount = user.FollowerCount,
                FollowingCount = user.FollowingCount,
                IsFollowing = isFollowing,
            };
            return profile;
        }

        public async Task<List<Recipe>> GetRecipes(string currentUserId, string userID, Pagination pagination)
        {
            var currentUser = (await _mongoDBService.Users.FindAsync(u => u.AccountID == currentUserId)).FirstOrDefault() ?? throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            var user = (await _mongoDBService.Users.FindAsync(u => u.AccountID == userID)).FirstOrDefault() ?? throw new NovaanException(ErrorCodes.PROFILE_USER_NOT_FOUND, HttpStatusCode.NotFound);
            var recipes = new List<Recipe>();
            if (currentUser.AccountID == user.AccountID)
            {
                // get all recipes of the user
                recipes = (await _mongoDBService.Recipes.FindAsync(r => r.CreatorId == user.AccountID)).ToList();
            }
            else
            {
                // get public recipes of the user
                recipes = (await _mongoDBService.Recipes.FindAsync(r => r.CreatorId == user.AccountID && r.Status == Status.Approved)).ToList();
            }

            return recipes.Skip(pagination.Start).Take(pagination.Limit).ToList();
        }
    }
}

