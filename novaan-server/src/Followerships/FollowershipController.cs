﻿using System;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoConnector;
using MongoConnector.Models;
using NovaanServer.src.Common.DTOs;
using NovaanServer.src.Common.Utils;
using NovaanServer.src.ExceptionLayer.CustomExceptions;
using NovaanServer.src.Followerships.DTOs;

namespace NovaanServer.src.Followerships
{
    [Authorize]
    [ApiController]
    public class FollowershipController : ControllerBase
    {
        private readonly IFollowershipService _followershipService;
        public FollowershipController(IFollowershipService followershipService)
        {
            _followershipService = followershipService;
        }

        [HttpPost("api/follow/{userId}")]
        public async Task<IActionResult> FollowUser(string userId)
        {
            var currentUserID = Request.GetUserId() ?? throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            await _followershipService.FollowUser(currentUserID, userId);
            return Ok();
        }

        [HttpPost("api/unfollow/{userId}")]
        public async Task<IActionResult> UnfollowUser(string userId)
        {
            var currentUserID = Request.GetUserId() ?? throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound);
            await _followershipService.UnfollowUser(currentUserID, userId);
            return Ok();
        }

        [HttpGet("api/followers/{userId}")]
        public List<FollowershipDTO> GetFollowers(string userId, [FromQuery] Pagination pagination)
        {
            return _followershipService.GetFollowers(userId, pagination);
        }

        [HttpGet("api/following/{userId}")]
        public List<FollowershipDTO> GetFollowing(string userId, [FromQuery] Pagination pagination)
        {
            return _followershipService.GetFollowing(userId, pagination);
        }

    }
}

