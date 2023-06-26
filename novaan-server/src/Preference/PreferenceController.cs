using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaanServer.src.Common.Utils;
using NovaanServer.src.ExceptionLayer.CustomExceptions;
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
        public AllPreferencesDTO GetAllPreferences()
        {
            var result = _preferenceService.GetAllPreferences();
            return result;
        }

        [HttpGet("me")]
        public async Task<UserPreferenceDTO> GetUserPreferences()
        {
            var currentUserId = Request.GetUserId();
            var result = await _preferenceService.GetUserPreferences(currentUserId);
            return result;
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