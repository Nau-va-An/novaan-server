using System;
using MongoConnector.Models;
using NovaanServer.src.Common.DTOs;
using NovaanServer.src.Profile.DTOs;

namespace NovaanServer.src.Profile
{
    public interface IProfileService
    {
        public Task<ProfileRESDTO> GetProfile(string currentUserId, string userID);
        public Task<List<Recipe>> GetRecipes(string currentUserId, string userID, Pagination pagination);
        public Task<List<SavedPost>> GetSavedPosts(string currentUserId, string userID, Pagination pagination);
        public Task<List<CulinaryTip>> GetTips(string currentUserId, string userID, Pagination pagination);
    }
}

