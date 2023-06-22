using System;
using System.Net;
using MongoConnector;
using MongoConnector.Models;
using MongoDB.Driver;
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

        public async Task<ProfileRESDTO> GetProfile(string currentUserId,string userID)
        {
			User currentUser = (await _mongoDBService.Users.FindAsync(u => u.Id == currentUserId)).FirstOrDefault() ?? throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            User user = (await _mongoDBService.Users.FindAsync(u => u.Id == userID)).FirstOrDefault() ?? throw new NovaanException(ErrorCodes.PROFILE_USER_NOT_FOUND, HttpStatusCode.NotFound);
			// check if current user is following the user
			bool isFollowing = (await _mongoDBService.Followerships.FindAsync(f => f.FollowerId == currentUserId && f.FollowingId == userID)).Any();
            List<Recipe> recipeList = (await _mongoDBService.Recipes.FindAsync(r => r.CreatorId == user.AccountID)).ToList();
            var profile = new ProfileRESDTO
            {
                UserName = user.DisplayName,
                UserID = user.Id,
                Avatar = user.ProfilePicture,
                RecipeList = recipeList,
                FollowersCount = user.FollowerCount,
                FollowingCount = user.FollowingCount,
				IsFollowing = isFollowing,
            };
            return profile;
        }
    }
}

