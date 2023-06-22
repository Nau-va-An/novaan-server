using System;
using Microsoft.AspNetCore.Mvc;
using MongoConnector;
using MongoConnector.Models;
using NovaanServer.src.Followerships.DTOs;

namespace NovaanServer.src.Followerships
{
    [ApiController]
    public class FollowershipController : ControllerBase
    {
        private readonly IFollowershipService _followershipService;
        public FollowershipController(IFollowershipService followershipService)
        {
            _followershipService = followershipService;
        }

        [HttpPost("api/follow")]
        public async Task<IActionResult> FollowUser([FromBody] FollowershipDTO followUserDTO)
        {
            await _followershipService.FollowUser(followUserDTO);
            return Ok();
        }

        [HttpPost("api/unfollow")]
        public async Task<IActionResult> UnfollowUser([FromBody] FollowershipDTO followUserDTO)
        {
            await _followershipService.UnfollowUser(followUserDTO);
            return Ok();
        }

        [HttpGet("api/followers/{userId}")]
        public List<User> GetFollowers(string userId)
        {
            return _followershipService.GetFollowers(userId);
        }

        [HttpGet("api/following/{userId}")]
        public List<User> GetFollowing(string userId)
        {
            return _followershipService.GetFollowing(userId);
        }

    }
}

