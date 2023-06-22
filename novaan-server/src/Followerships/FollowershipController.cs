using System;
using Microsoft.AspNetCore.Mvc;
using MongoConnector;
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

        /// <summary>
        /// Unfollow a user
        /// </summary>
        /// <param name="followUserDTO"></param>
        /// <returns></returns>
        [HttpPost("api/unfollow")]
        public async Task<IActionResult> UnfollowUser([FromBody] FollowershipDTO followUserDTO)
        {
            await _followershipService.UnfollowUser(followUserDTO);
            return Ok();
        }
    }
}

