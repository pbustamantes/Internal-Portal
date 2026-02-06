using System.Net;
using System.Text.Json;
using InternalPortal.Application.Common.Exceptions;
using InternalPortal.Domain.Exceptions;

namespace InternalPortal.API.Middleware;

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, response) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse("Validation Error", validationEx.Message, validationEx.Errors)),

            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                new ErrorResponse("Not Found", notFoundEx.Message)),

            ForbiddenException forbiddenEx => (
                HttpStatusCode.Forbidden,
                new ErrorResponse("Forbidden", forbiddenEx.Message)),

            DomainException domainEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse("Domain Error", domainEx.Message)),

            ApplicationException appEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse("Application Error", appEx.Message)),

            _ => (
                HttpStatusCode.InternalServerError,
                new ErrorResponse("Server Error", "An unexpected error occurred."))
        };

        context.Response.StatusCode = (int)statusCode;
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }
}

public record ErrorResponse(string Title, string Detail, object? Errors = null);
