using System;
namespace NovaanServer.src.Common.DTOs
{
	public class BaseResponse
	{
		public bool Success { get; set; }

        public List<string>? Errors { get; set; }
    }
}

