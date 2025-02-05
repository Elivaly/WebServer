using System.Net;
using System.Security.Authentication;

namespace AuthService.Exceptions;

public class ExceptionHandlerMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext); //продолжаем цепочку вызовов
        }
        catch (AuthenticationException ex)
        {
            await HandleExceptionAsync(httpContext, ex, HttpStatusCode.Unauthorized);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex, HttpStatusCode.InternalServerError);
        }
    }

    public async Task HandleExceptionAsync(HttpContext httpContext, Exception exception, HttpStatusCode httpStatus)
    {
        string msg = $"code: {httpStatus}   trace: {exception.StackTrace}";

        Console.WriteLine(exception.ToString());
        Console.WriteLine(msg);
    }
}
