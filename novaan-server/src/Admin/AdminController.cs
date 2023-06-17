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
        public List<SubmissionsDTO> GetSubmissions([FromQuery] Status status)
        {
            return _submissionService.GetSubmissions(status);
        }

        [HttpPut("status")]
        public IActionResult UpdateStatus([FromBody] StatusDTO statusDTO)
        {
            if (statusDTO.SubmissionType == SubmissionType.Recipe)
            {
                _submissionService.UpdateStatus<Recipe>(statusDTO);
            }
            else
            {
                _submissionService.UpdateStatus<CulinaryTip>(statusDTO);
            }
            return Ok();
        }
    }
}

