using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaanServer.src.Common.Utils;
using NovaanServer.src.Preference.DTOs;

namespace NovaanServer.src.Preference
{
    [Route("api/preference")]
    [ApiController]
    [Authorize]
    public class PreferenceController : ControllerBase
    {
        private readonly IPreferenceService _preferenceService;
        public PreferenceController(IPreferenceService preferenceService)
        {
            _preferenceService = preferenceService;
        }

        [HttpGet("all")]
        public async Task<AllPreferencesDTO> GetAllPreferences()
        {
            var preferences = await _preferenceService.GetAllPreferences();
            return preferences;
        }

        [HttpGet("me")]
        public async Task<UserPreferenceDTO> GetUserPreferences()
        {
            var currentUserId = Request.GetUserId();
            var userPreferences = await _preferenceService.GetUserPreferences(currentUserId);
            return userPreferences;
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateUserPreferences([FromBody] UserPreferenceDTO userPreferenceDTO)
        {
            var currentUserId = Request.GetUserId();
            await _preferenceService.UpdateUserPreferences(currentUserId, userPreferenceDTO);
            return Ok();
        }
    }

}