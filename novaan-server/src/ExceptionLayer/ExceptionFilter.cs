using System;
using System.Linq.Expressions;
using System.Net;
using NovaanServer.ExceptionLayer.CustomExceptions;
using NovaanServer.src.ExceptionLayer.CustomExceptions;
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
            response.StatusCode = exception switch
            {
                NovaanException ex => (int)ex.StatusCode,
                _ => (int)HttpStatusCode.InternalServerError,
            };

            var responseMessage = CustomJson.Stringify<BaseErrResponse>(
                exception switch
                {
                    NovaanException ex => new BaseErrResponse
                    {
                        Success = false,
                        ErrCode = ex.ErrCode,
                        Message = ErrorCodes.ErrorNameDictionary.GetValueOrDefault(ex.ErrCode)
                    },
                    FileNotFoundException ex => new BaseErrResponse
                    {
                        Success = false,
                        ErrCode = ErrorCodes.S3_FILE_NOT_FOUND,
                        Message = ex.Message
                    },
                    Exception ex => new BaseErrResponse
                    {
                        Success = false,
                        ErrCode = ErrorCodes.SERVER_UNAVAILABLE,
                        Message = ex.Message
                    }
                });

            // Log the error for later debugging and inspection
            _logger.LogError("Error occured at {endpoint} with stack trade: {message}",
                context.GetEndpoint(),
                exception.StackTrace
            );

            await context.Response.WriteAsync(responseMessage);
        }

        private static string GetPropertyName<T>(Expression<Func<T>> propertyLamba)
        {
            var memberExpress = propertyLamba.Body as MemberExpression;
            if (memberExpress == null)
            {
                throw new ArgumentException("Must pass a lambda");
            }
            return memberExpress.Member.Name;
        }
    }
}

