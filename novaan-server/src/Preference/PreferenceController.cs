using Microsoft.AspNetCore.Mvc;
using NovaanServer.src.Preference.DTOs;

namespace NovaanServer.src.Preference
{
    [Route("api/preference")]
    [ApiController]
    public class PreferenceController :ControllerBase
    {
        private readonly IPreferenceService _preferenceService;
        public PreferenceController(IPreferenceService preferenceService)
        {
            _preferenceService = preferenceService;
        }

        [HttpGet("all")]
        public PreferenceDTO GetAllPreference()
        {
            var result =  _preferenceService.GetAllPreferences();
            return result;
        }

        // get preference for user id
        [HttpGet("user")]
        public UserPreferenceDTO GetPreference([FromBody] string userId)
        {
            var result =  _preferenceService.GetPreference(userId);
            return result;
        }

        // update preference for user id with preference id
        // [HttpPut]
        // public async Task<IActionResult> UpdatePreference(string userId, string dietId, string cuisineId, string mealTypeId, string dishTypeId)
        // {
        //     await _preferenceService.UpdatePreference(userId, dietId, cuisineId, mealTypeId, dishTypeId);
        //     return Ok();
        // }
    }
    
}