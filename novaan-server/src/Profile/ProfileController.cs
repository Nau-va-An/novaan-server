using System;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoConnector.Models;
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

        [HttpGet("/profile/{userID}")]
        public async Task<ProfileRESDTO> GetProfile(string userID)
        {
            var currentUserId = Request.GetUserId()?? throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            return await _profileService.GetProfile(currentUserId,userID);
        }
    }
}

