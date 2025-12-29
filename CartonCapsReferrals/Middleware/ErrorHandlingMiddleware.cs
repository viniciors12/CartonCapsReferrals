using CartonCapsReferrals.Api.Utils.Exceptions;
using System.Text.Json;
using static CartonCapsReferrals.Api.Utils.Exceptions.ReferralExceptions;

namespace CartonCapsReferrals.Api.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (DomainException ex)
            {
                await HandleDomainException(context, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                await WriteError(context, 500, $"An unexpected error occurred, {ex.Message} {ex.InnerException}");
            }
        }

        private Task HandleDomainException(HttpContext context, DomainException ex)
        {
            int status = ex switch
            {
                NotFoundException => 404,
                ForbiddenException => 403,
                BadRequestException => 400,
                _ => 400
            };

            return WriteError(context, status, ex.Message);
        }

        private Task WriteError(HttpContext context, int status, string message)
        {
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/json";

            return context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = message,
                status
            }));
        }
    }
}
