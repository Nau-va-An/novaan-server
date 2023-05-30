using System;
using MongoConnector;
using Microsoft.AspNetCore.Mvc;

namespace NovaanServer.Developer
{
	[Route("api/dev")]
	[ApiController]
	public class DeveloperController: Controller
	{
		private readonly MongoDBService _mongoService;

		public DeveloperController(MongoDBService mongoDBService)
		{
			_mongoService = mongoDBService;
		}

        [HttpGet("health")]
        public IActionResult GetHealthCheck()
        {
            return Ok("Server is up and running");
        }

        [HttpGet("database-health")]
		public IActionResult GetDatabaseHealthCheck()
		{
			if(_mongoService.PingDatabase())
			{
                return Ok("Database is up and running");
            }

			return BadRequest();
        }
	}
}

