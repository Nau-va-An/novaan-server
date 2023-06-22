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
        public async Task<IActionResult> FollowUser([FromBody] FollowUserDTO followUserDTO)
        {
            await _followershipService.FollowUser(followUserDTO);
            return Ok();
        }
    }
}

