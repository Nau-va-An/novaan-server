using System;
using System.Net;
using NovaanServer.ExceptionLayer.CustomExceptions;
using Utils.Json;

namespace NovaanServer.ExceptionLayer
{
    public class ExceptionFilter
    {
        private readonly RequestDelegate _next;

        private readonly ILogger<ExceptionFilter> _logger;

        public ExceptionFilter(RequestDelegate next, ILogger<ExceptionFilter> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception e)
            {
                await HandleExceptionAsync(context, e);
            }
        }

        public async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Filter exceptions to derive status code
            var response = context.Response;
            response.ContentType = "application/json";
            switch (exception)
            {
                case BadHttpRequestException ex:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                case UnauthorizedAccessException ex:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            // Create custom response based on pre-defined format
            var customResponse = new BaseErrResponse()
            {
                Success = false,
                Body = new
                {
                    Message = exception.Message
                }
            };

            // Log the error for later debugging and inspection
            _logger.LogError("Error occured at {endpoint} with message: {message}",
                context.GetEndpoint(),
                exception.Message
            );

            var result = CustomJson.Stringify<BaseErrResponse>(customResponse);
            await context.Response.WriteAsync(result);
        }
    }
}

