using System.Net;
using Microsoft.AspNetCore.Mvc;
using NovaanServer.src.Common.Utils;
using NovaanServer.src.ExceptionLayer.CustomExceptions;
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

        public object ErrorCode { get; private set; }

        [HttpGet("all")]
        public AllPreferencesDTO GetAllPreferences()
        {
            var result = _preferenceService.GetAllPreferences();
            return result;
        }

        [HttpGet("user/{userId}")]
        public async Task<UserPreferenceDTO> GetUserPreferences()
        {
            var currentUserId = Request.GetUserId()?? throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            var result = await _preferenceService.GetUserPreferences(currentUserId);
            return result;
        }

        [HttpPut("user/{userId}")]
        public async Task<IActionResult> UpdateUserPreferences([FromBody] UserPreferenceDTO userPreferenceDTO)
        {
            var currentUserId = Request.GetUserId()?? throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            await _preferenceService.UpdateUserPreferences(currentUserId, userPreferenceDTO);
            return Ok();
        }
    }

}