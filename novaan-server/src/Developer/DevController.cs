using System;
using System.IO;
using MongoConnector;
using S3Connector;
using Microsoft.AspNetCore.Mvc;

namespace NovaanServer.Developer
{
    [Route("api/dev")]
    [ApiController]
    public class DevController : Controller
    {
        private readonly IDevService _devService;

        public DevController(IDevService devService)
        {
            _devService = devService;
        }

        [HttpGet("health")]
        public IActionResult GetHealthCheck()
        {
            if(_devService.IsServerHealthy())
            {
                return Ok("Server is up and running");
            }

            // Should never run to here
            return StatusCode(500, "Cannot connect to server");
        }

        [HttpGet("health/db")]
        public IActionResult GetDbHealth()
        {
            if(_devService.IsDatabaseHealthy())
            {
                return Ok("Database is up and running");
            }

            return StatusCode(500, "Cannot connect to database");
        }

        [HttpGet("health/s3")]
        public IActionResult GetS3Health()
        {
            if(_devService.IsS3Healthy())
            {
                return Ok("S3 Bucket is up and running");
            }

            return StatusCode(500, "Cannot connect to S3 Bucket");
        }

        // TODO: Test endpoint. Clear content before commit
        [HttpGet("exec")]
        public async Task<IActionResult> Execute()
        {
            return Ok();
        }
    }
}

