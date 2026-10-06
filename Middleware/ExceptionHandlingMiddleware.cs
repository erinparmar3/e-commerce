using System.Net;
using System.Text.Json;
using ECommerceApp.Common;

namespace ECommerceApp.Middleware;

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
        catch (AppException ex)
        {
            _logger.LogWarning(ex, "Handled application exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled server error occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, (int)HttpStatusCode.InternalServerError, "An unexpected internal server error occurred.");
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, int statusCode, string message)
    {
        if (context.Request.Path.StartsWithSegments("/api") || (context.Request.Headers.Accept.ToString().Contains("application/json") && !context.Request.Headers.Accept.ToString().Contains("text/html")))
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new
            {
                success = false,
                status = statusCode,
                message = message
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
        else
        {
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.StatusCode = statusCode;
            var safeMessage = WebUtility.HtmlEncode(message);
            var html = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Error {statusCode} - ShopNet</title>
    <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css"" rel=""stylesheet"" />
    <link rel=""stylesheet"" href=""https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css"" />
</head>
<body class=""bg-light d-flex align-items-center min-vh-100"">
    <div class=""container"">
        <div class=""row justify-content-center"">
            <div class=""col-md-6 text-center"">
                <div class=""card border-0 shadow-sm rounded-4 p-5"">
                    <div class=""text-danger mb-3""><i class=""bi bi-exclamation-triangle-fill display-1""></i></div>
                    <h2 class=""fw-bold text-dark mb-2"">Oops! Something went wrong</h2>
                    <p class=""text-muted fs-6 mb-4"">{safeMessage}</p>
                    <div class=""d-flex justify-content-center gap-3"">
                        <a href=""/"" class=""btn btn-primary px-4 rounded-pill fw-medium""><i class=""bi bi-house me-2""></i>Return Home</a>
                        <button onclick=""history.back()"" class=""btn btn-outline-secondary px-4 rounded-pill"">Go Back</button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</body>
</html>";
            await context.Response.WriteAsync(html);
        }
    }
}
