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
        
        /// <summary>
        /// Get all preference
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public PreferenceDTO GetAllPreference()
        {
            var result =  _preferenceService.GetAllPreferences();
            return result;
        }

        // update preference for user id with preference id
        [HttpPut]
        public async Task<IActionResult> UpdatePreference()
        {
            await _preferenceService.UpdatePreference();
            return Ok();
        }
    }
    
}