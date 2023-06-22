using System;
using NovaanServer.src.Common.DTOs;

namespace NovaanServer.src.Developer.DTOs
{
    public class HealthCheckRes : BaseResponse
    {
        public string Message { get; set; }
    }
}

