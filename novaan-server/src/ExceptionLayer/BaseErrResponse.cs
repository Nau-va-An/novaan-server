using System;
namespace NovaanServer.ExceptionLayer.CustomExceptions
{
	public class BaseErrResponse
	{
        public bool Success { get; set; }

		public object? Body { get; set; }
	}
}

