using System;
using System.IO;
using MongoConnector;
using S3Connector;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NovaanServer.src.Common.Attributes;
using NovaanServer.src.Developer.DTOs;
using System.Net;

namespace NovaanServer.Developer
{
    [AllowAnonymous]
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
        public HealthCheckRes GetHealthCheck()
        {
            if(_devService.IsServerHealthy())
            {
                Response.StatusCode = (int) HttpStatusCode.OK;
                return new HealthCheckRes
                {
                    Message = "Server is up running"
                };
            }

            // Should never run to here
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return new HealthCheckRes
            {
                Message = "Cannot connect to Novaan Server"
            };
        }

        [DevelopmentOnly]
        [HttpGet("health/db")]
        public HealthCheckRes GetDbHealth()
        {
            if (_devService.IsServerHealthy())
            {
                Response.StatusCode = (int)HttpStatusCode.OK;
                return new HealthCheckRes
                {
                    Message = "MongoDB Atlas is up and running"
                };
            }

            // Should never run to here
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return new HealthCheckRes
            {
                Message = "Cannot connect to MongoDB Atlas"
            };
        }

        [DevelopmentOnly]
        [HttpGet("health/s3")]
        public HealthCheckRes GetS3Health()
        {
            if (_devService.IsServerHealthy())
            {
                Response.StatusCode = (int)HttpStatusCode.OK;
                return new HealthCheckRes
                {
                    Message = "AWS S3 Bucket is up and running"
                };
            }

            // Should never run to here
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return new HealthCheckRes
            {
                Message = "Cannot connect to AWS S3 Bucket"
            };
        }

        [DevelopmentOnly]
        [HttpGet("exec")]
        public async Task<IActionResult> Execute()
        {
            return Ok();
        }
    }
}

