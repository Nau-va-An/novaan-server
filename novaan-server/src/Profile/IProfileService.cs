using System;
using MongoConnector.Models;
using NovaanServer.src.Common.DTOs;
using NovaanServer.src.Profile.DTOs;

namespace NovaanServer.src.Profile
{
    public interface IProfileService
    {
        public Task<GetProfileResDTO> GetProfile(string? currentUserId, string userId);
        public Task<List<Recipe>> GetRecipes(string? currentUserId, string userId, Pagination pagination);
        public Task<List<SavedPost>> GetSavedPosts(string? currentUserId, string userId, Pagination pagination);
        public Task<List<CulinaryTip>> GetTips(string? currentUserId, string userId, Pagination pagination);
    }
}

