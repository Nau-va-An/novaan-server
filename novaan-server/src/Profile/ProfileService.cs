﻿using System;
using System.Net;
using MongoConnector;
using MongoConnector.Enums;
using MongoConnector.Models;
using MongoDB.Driver;
using NovaanServer.src.Common.DTOs;
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
            User currentUser = (await _mongoDBService.Users.FindAsync(u => u.AccountID == currentUserId)).FirstOrDefault() ?? throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            User user = (await _mongoDBService.Users.FindAsync(u => u.Id == userID)).FirstOrDefault() ?? throw new NovaanException(ErrorCodes.PROFILE_USER_NOT_FOUND, HttpStatusCode.NotFound);
            // check if current user is following the user
            bool isFollowing = (await _mongoDBService.Followerships.FindAsync(f => f.FollowerId == currentUser.Id && f.FollowingId == userID)).Any();
            var recipeList = await GetRecipes(currentUserId, userID, new Pagination { Start = 0, Limit = 10 });
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

        public async Task<List<Recipe>> GetRecipes(string currentUserId, string userID, Pagination pagination)
        {
            var currentUser = (await _mongoDBService.Users.FindAsync(u => u.AccountID == currentUserId)).FirstOrDefault() ?? throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            var user = (await _mongoDBService.Users.FindAsync(u => u.Id == userID)).FirstOrDefault() ?? throw new NovaanException(ErrorCodes.PROFILE_USER_NOT_FOUND, HttpStatusCode.NotFound);
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

        public async Task<List<SavedPost>> GetSavedPosts(string currentUserId, string userID,Pagination pagination)
        {
            var currentUser = (await _mongoDBService.Users.FindAsync(u => u.AccountID == currentUserId)).FirstOrDefault() ?? throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            var user = (await _mongoDBService.Users.FindAsync(u => u.Id == userID)).FirstOrDefault() ?? throw new NovaanException(ErrorCodes.PROFILE_USER_NOT_FOUND, HttpStatusCode.NotFound);
             // Only current user can see his/her saved posts
            if (currentUser.Id != user.Id)
            {
                // forbidden content
                throw new NovaanException(ErrorCodes.FORBIDDEN_PROFILE_CONTENT, HttpStatusCode.Forbidden);
            }
            var savedPost = user.SavedPost.Skip(pagination.Start).Take(pagination.Limit).ToList();
            return savedPost;
        }

        public async Task<List<CulinaryTip>> GetTips(string currentUserId, string userID, Pagination pagination)
        {
            var currentUser = (await _mongoDBService.Users.FindAsync(u => u.AccountID == currentUserId)).FirstOrDefault() ?? throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            var user = (await _mongoDBService.Users.FindAsync(u => u.Id == userID)).FirstOrDefault() ?? throw new NovaanException(ErrorCodes.PROFILE_USER_NOT_FOUND, HttpStatusCode.NotFound);
            var tips = new List<CulinaryTip>();
            if (currentUser.AccountID == user.AccountID)
            {
                // get all tips of the user
                tips = (await _mongoDBService.CulinaryTips.FindAsync(t => t.CreatorId == user.AccountID)).ToList();
            }
            else
            {
                // get public tips of the user
                tips = (await _mongoDBService.CulinaryTips.FindAsync(t => t.CreatorId == user.AccountID && t.Status == Status.Approved)).ToList();
            }
            return tips.Skip(pagination.Start).Take(pagination.Limit).ToList();
        }
    }
}

