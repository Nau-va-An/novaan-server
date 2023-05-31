using System;
using System.IO;
using MongoConnector;
using S3Connector;
using Microsoft.AspNetCore.Mvc;

namespace NovaanServer.Developer
{
    [Route("api/dev")]
    [ApiController]
    public class DeveloperController : Controller
    {
        private readonly MongoDBService _mongoService;
        private readonly S3Service _s3Service;

        public DeveloperController(MongoDBService mongoDBService, S3Service s3Service)
        {
            _mongoService = mongoDBService;
            _s3Service = s3Service;
        }

        [HttpGet("health")]
        public IActionResult GetHealthCheck()
        {
            return Ok("Server is up and running");
        }

        [HttpGet("database-health")]
        public IActionResult GetDatabaseHealthCheck()
        {
            if (_mongoService.PingDatabase())
            {
                return Ok("Database is up and running");
            }

            return BadRequest();
        }

        // TODO: Test endpoint. Clear content before commit
        [HttpGet("exec")]
        public async Task<IActionResult> Execute()
        {
            return Ok();
        }
    }
}

