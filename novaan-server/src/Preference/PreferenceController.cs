using Microsoft.AspNetCore.Mvc;
using NovaanServer.src.Preference.DTOs;

namespace NovaanServer.src.Preference
{
    [Route("api/preference")]
    [ApiController]
    public class PreferenceController : ControllerBase
    {
        private readonly IPreferenceService _preferenceService;
        public PreferenceController(IPreferenceService preferenceService)
        {
            _preferenceService = preferenceService;
        }

        [HttpGet("all")]
        public AllPreferencesDTO GetAllPreferences()
        {
            var result = _preferenceService.GetAllPreferences();
            return result;
        }

        [HttpGet("user/{userId}")]
        public async Task<UserPreferenceDTO> GetUserPreferences(string userId)
        {
            var result = await _preferenceService.GetUserPreferences(userId);
            return result;
        }

        [HttpPut("user/{userId}")]
        public async Task<IActionResult> UpdateUserPreferences([FromBody] UserPreferenceDTO userPreferenceDTO, string userId)
        {
            await _preferenceService.UpdateUserPreferences(userId, userPreferenceDTO);
            return Ok();
        }
    }

}