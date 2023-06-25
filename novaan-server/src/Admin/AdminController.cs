using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoConnector.Enums;
using MongoConnector.Models;
using NovaanServer.src.Admin.DTOs;

namespace NovaanServer.src.Admin
{
    [Route("api/admin")]
    [ApiController]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _submissionService;

        public AdminController(IAdminService submissionService)
        {
            _submissionService = submissionService;
        }

        [HttpGet("submissions")]
        public SubmissionsDTO GetSubmissions([FromQuery] List<Status> status)
        {
            return _submissionService.GetSubmissions(status);
        }

        [HttpPut("status/{type}")]
        public IActionResult UpdateStatus([FromBody] StatusDTO statusDTO, SubmissionType type)
        {
            if (type == SubmissionType.Recipe)
            {
                _submissionService.UpdateStatus<Recipe>(statusDTO, MongoCollections.Recipes);
            }
            else
            {
                _submissionService.UpdateStatus<CulinaryTip>(statusDTO, MongoCollections.CulinaryTips);
            }

            return Ok();
        }
    }
}

