using System;
namespace NovaanServer.src.ExceptionLayer.CustomExceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException() : base() { }

        public NotFoundException(string message) : base(message) { }
    }
}

