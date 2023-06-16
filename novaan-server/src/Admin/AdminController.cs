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

        [HttpGet("submissions/{status}")]
        public async Task<IActionResult> GetSubmissions([FromQuery] Status status)
        {
            var submissions = _submissionService.GetSubmissions((int)status);
            return Ok(submissions);
        }

        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] StatusDTO statusDTO)
        {
            if (statusDTO.SubmissionType == SubmissionType.Recipe)
            {
                _submissionService.UpdateStatus<Recipe>(statusDTO);
            }else{
                _submissionService.UpdateStatus<CulinaryTip>(statusDTO);
            }
            return Ok();
        }
    }
}

