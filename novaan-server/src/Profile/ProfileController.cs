using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoConnector.Models;
using NovaanServer.src.Common.DTOs;
using NovaanServer.src.Common.Utils;
using NovaanServer.src.Profile.DTOs;

namespace NovaanServer.src.Profile
{
    [Route("api/profile")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet("{userId}")]
        public async Task<GetProfileResDTO> GetProfile(string userId)
        {
            var currentUserId = Request.GetUserId();
            return await _profileService.GetProfile(currentUserId, userId);
        }

        // Get recipes of a user
        [HttpGet("{userId}/recipes")]
        public async Task<List<Recipe>> GetRecipes(string userId, [FromQuery] Pagination pagination)
        {
            var currentUserId = Request.GetUserId();
            return await _profileService.GetRecipes(currentUserId, userId, pagination);
        }

        // Get tips of a user
        [HttpGet("{userId}/tips")]
        public async Task<List<CulinaryTip>> GetTips(string userId, [FromQuery] Pagination pagination)
        {
            var currentUserId = Request.GetUserId();
            return await _profileService.GetTips(currentUserId, userId, pagination);
        }

        // Get saved post of a user
        [HttpGet("{userId}/saved")]
        public async Task<List<SavedPost>> GetSavedPosts(string userId, [FromQuery] Pagination pagination)
        {
            // only current user can see his/her saved posts
            var currentUserId = Request.GetUserId();
            return await _profileService.GetSavedPosts(currentUserId, userId, pagination);
        }

    }
}

