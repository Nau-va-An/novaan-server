﻿using System;
using Microsoft.AspNetCore.Mvc;
using MongoConnector.Models;

namespace NovaanServer.src.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _submissionService;

        public AdminController(IAdminService submissionService)
        {
            _submissionService = submissionService;
        }

        /// <summary>
        /// View all recipe and culinary tips from users, if status is not specified, return pending status
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpGet("get-submissions")]
        public async Task<IActionResult> GetSubmissions([FromQuery] int? status)
        {
            if (status == null)
            {
                status = 0;
            }
            // Calling Service to get list of submissions
            var submissions = _submissionService.GetSubmissions(status.Value);
            return Ok(submissions);
        }

        /// <summary>
        /// Update status of recipe 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPut("status/{submissionType}")]
        public async Task<IActionResult> UpdateStatus(string submissionType, [FromForm] string id, [FromForm] int status)
        {
            switch (submissionType)
            {
                case "recipes":
                    await _submissionService.UpdateStatus<Recipe>(submissionType,id, status);
                    break;
                case "culinaryTips":
                    await _submissionService.UpdateStatus<CulinaryTip>(submissionType,id, status);
                    break;
                default:
                    return BadRequest("Invalid submission type");
            }
            return Ok();
        }
    }
}

