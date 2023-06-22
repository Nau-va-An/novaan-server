using System;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoConnector.Models;
using NovaanServer.src.Common.DTOs;
using NovaanServer.src.Common.Utils;
using NovaanServer.src.ExceptionLayer.CustomExceptions;
using NovaanServer.src.Profile.DTOs;

namespace NovaanServer.src.Profile
{
    [Authorize]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet("api/profile/{userID}")]
        public async Task<ProfileRESDTO> GetProfile(string userID)
        {
            var currentUserId = Request.GetUserId() ?? throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            return await _profileService.GetProfile(currentUserId, userID);
        }

        // Get recipes of a user
        [HttpGet("api/profile/{userID}/recipes")]
        public async Task<List<Recipe>> GetRecipes(string userID, [FromQuery] Pagination pagination)
        {
            var currentUserId = Request.GetUserId() ?? throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            return await _profileService.GetRecipes(currentUserId, userID, pagination);
        }

        // Get tips of a user
        [HttpGet("api/profile/{userID}/tips")]
        public async Task<List<CulinaryTip>> GetTips(string userID, [FromQuery] Pagination pagination)
        {
            var currentUserId = Request.GetUserId() ?? throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            return await _profileService.GetTips(currentUserId, userID, pagination);
        }

        // Get saved post of a user
        [HttpGet("api/profile/{userID}/saved")]
        public async Task<List<SavedPost>> GetSavedPosts(string userID, [FromQuery] Pagination pagination)
        {
            // only current user can see his/her saved posts
            var currentUserId = Request.GetUserId() ?? throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            return await _profileService.GetSavedPosts(currentUserId, userID, pagination);
        }

    }
}

