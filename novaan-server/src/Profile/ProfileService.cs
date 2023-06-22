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

        public async Task<ProfileRESDTO> GetMyProfile(string currentUser)
        {
			User user = (await _mongoDBService.Users.FindAsync(u => u.AccountID == currentUser)).FirstOrDefault() ?? throw new NovaanException(ErrorCodes.PROFILE_USER_NOT_FOUND, HttpStatusCode.NotFound);
			List<Recipe> recipeList = (await _mongoDBService.Recipes.FindAsync(r => r.CreatorId == currentUser)).ToList();
			var profile = new ProfileRESDTO
			{
				UserName = user.DisplayName,
				UserID = user.Id,
				Avatar = user.ProfilePicture,
				RecipeList = recipeList,
				FollowersCount = user.FollowerCount,
				FollowingCount = user.FollowingCount,
			};
			return profile;
        }

        public ProfileRESDTO GetProfile(string userID, string userID1)
        {
            throw new NotImplementedException();
        }
    }
}

