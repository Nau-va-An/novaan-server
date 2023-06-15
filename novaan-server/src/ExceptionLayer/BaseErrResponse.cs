using System;
using System.Text.Json.Serialization;

namespace NovaanServer.ExceptionLayer.CustomExceptions
{
	public class BaseErrResponse
	{
		[JsonPropertyName("success")]
        public bool Success { get; set; }

		[JsonPropertyName("body")]
		public object? Body { get; set; }
	}
}

