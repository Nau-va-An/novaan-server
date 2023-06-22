using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using MongoConnector.Models;
using NovaanServer.src.Common.Utils;
using NovaanServer.src.ExceptionLayer.CustomExceptions;
using NovaanServer.src.Profile.DTOs;

namespace NovaanServer.src.Profile
{
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet("/profile/{userID}")]
        public ProfileRESDTO GetProfile(string userID)
        {
            var currentUser = Request.GetUserId()?? throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            return _profileService.GetProfile(currentUser, userID);
        }

        // Get my profile
        [HttpGet("/profile/me")]
        public async Task<ProfileRESDTO> GetMyProfile()
        {
            var currentUser = Request.GetUserId()?? throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            var profile= await _profileService.GetMyProfile(currentUser);
            return profile;
        }
    }
}

