namespace AuthService.Middleware
{
    public class LoggingMiddleware
    {
            readonly RequestDelegate _next;
            public LoggingMiddleware(RequestDelegate next)
            { 
                _next = next; 
            } 
            public async Task InvokeAsync(HttpContext context) 
            { 
                var headers = context.Request.Headers;
                foreach (var header in headers)
                { 
                    Console.WriteLine($"{header.Key}: {header.Value}"); 
                }
                await _next(context); 
            } 
    }
}
