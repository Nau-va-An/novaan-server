using System;
using Microsoft.AspNetCore.Mvc;
using MongoConnector.Models;

namespace NovaanServer.src.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    public class SubmissionController : ControllerBase
    {
        private readonly ISubmissionService _submissionService;

        public SubmissionController(ISubmissionService submissionService)
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
        [HttpPut("status/recipe")]
        public async Task<IActionResult> UpdateStatus([FromQuery] string id, [FromQuery] int status)
        {
            // Calling Service to update status
            await _submissionService.UpdateStatus<Recipe>(id, status);
            return Ok();
        }

        /// <summary>
        /// Update status of culinary tips
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPut("status/culinary-tips")]
        public async Task<IActionResult> UpdateStatusCulinaryTips([FromQuery] string id, [FromQuery] int status)
        {
            // Calling Service to update status
            await _submissionService.UpdateStatus<CulinaryTips>(id, status);
            return Ok();
        }
    }
}

