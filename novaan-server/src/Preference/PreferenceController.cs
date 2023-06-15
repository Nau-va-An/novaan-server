using Microsoft.AspNetCore.Mvc;

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
        public IActionResult GetAllPreference()
        {
            var result =  _preferenceService.GetAllPreference();
            return Ok(result);
        }
    }
    
}