namespace Meter_Read_API.Middleware
{
    public class FileValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public FileValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        { 
            if (context.Request.Path.StartsWithSegments("/api/meterreading/upload", StringComparison.OrdinalIgnoreCase)
                && context.Request.Method == HttpMethods.Post)
            {
                if (!context.Request.HasFormContentType || !context.Request.Form.Files.Any())
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("No file uploaded in the request.");
                    return; 
                }
            }

            await _next(context);
        }
    }

}
