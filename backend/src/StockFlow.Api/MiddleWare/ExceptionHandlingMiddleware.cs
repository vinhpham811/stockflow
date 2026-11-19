using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.Common.Exceptions;
using StockFlow.Domain.Exceptions;

namespace StockFlow.Api.MiddleWare
{
    /// <summary>
    /// Bắt mọi exception, đổi thành HTTP status code phù hợp.
    /// Nhờ vậy Controller không cần try/catch.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            catch(Exception exception)
            {
                await HandleExceptionAysnc(context, exception);
            }
        }

        private async Task HandleExceptionAysnc(HttpContext context,Exception exception)
        {
            var statusCode = exception switch
            {
                BadRequestException => StatusCodes.Status400BadRequest,
                DomainException => StatusCodes.Status400BadRequest,
                UnauthorizedException => StatusCodes.Status401Unauthorized,
                NotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };
            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, "Unhandled exception");
            }
            // Không trả chi tiết lỗi 500 ra ngoài để tránh lộ thông tin hệ thống
            var message = statusCode == StatusCodes.Status500InternalServerError
            ? "An unexpected error occurred."
            : exception.Message;

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = message
            };
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(problem);

        }


    }
}
