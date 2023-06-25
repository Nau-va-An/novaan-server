using System;
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
            var currentUserId = Request.GetUserId();
            await _followershipService.FollowUser(currentUserId, userId);
            return Ok();
        }

        [HttpPost("api/unfollow/{userId}")]
        public async Task<IActionResult> UnfollowUser(string userId)
        {
            var currentUserId = Request.GetUserId();
            await _followershipService.UnfollowUser(currentUserId, userId);
            return Ok();
        }

        [HttpGet("api/followers/{userId}")]
        public async Task<List<FollowershipDTO>> GetFollowers(string userId, [FromQuery] Pagination pagination)
        {
            var currentUserId = Request.GetUserId();
            return await _followershipService.GetFollowers(currentUserId, userId, pagination);
        }

        [HttpGet("api/following/{userId}")]
        public async Task<List<FollowershipDTO>> GetFollowing(string userId, [FromQuery] Pagination pagination)
        {
            var currentUserID = Request.GetUserId();
            return await _followershipService.GetFollowing(currentUserID, userId, pagination);
        }

    }
}

