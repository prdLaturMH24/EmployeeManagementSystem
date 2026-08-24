using EmployeeManagementSystem.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace EmployeeManagementSystem.Common
{
    public static class GlobalExceptionHandler
    {
        public static void HandleException(IApplicationBuilder app)
        {
            var loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(nameof(GlobalExceptionHandler));

            app.Run(httpContext =>
            {
                var errorFeature = httpContext.Features.Get<IExceptionHandlerFeature>();
                if (errorFeature == null)
                {
                    logger.LogError(1, "Unknown error occurred");
                    return SetResponse(httpContext, HttpStatusCode.InternalServerError, "An unknown error occurred.");
                }

                switch(errorFeature.Error)
                {
                    case BadRequestException:
                        logger.LogError(1, errorFeature.Error, "Bad request error occurred.");
                        return SetResponse(httpContext, HttpStatusCode.BadRequest, errorFeature.Error.Message);
                    case NotFoundException:
                        logger.LogError(1, errorFeature.Error, "Resource not found error occurred.");
                        return SetResponse(httpContext, HttpStatusCode.NotFound, errorFeature.Error.Message);
                    default:
                        logger.LogError(1, errorFeature.Error, "An unknown error occurred.");
                        return SetResponse(httpContext, HttpStatusCode.InternalServerError, "An unknown error occurred.");
                }
            });
        }

        public static Task SetResponse(this HttpContext context, HttpStatusCode statusCode, string? message = null, string contentType = "text/plain")
        {
            context.Response.StatusCode = (int)statusCode;
            if (!string.IsNullOrEmpty(message))
            {
                context.Response.ContentType = contentType;
                return context.Response.WriteAsync(message);
            }
            return Task.CompletedTask;
        }
    }
}
