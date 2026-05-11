using System.Diagnostics;

namespace TherapyCenter.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var request = context.Request;

            _logger.LogInformation(
                "Incoming Request: {Method} {Url}",
                request.Method,
                request.Path
            );

            await _next(context);

            stopwatch.Stop();

            var response = context.Response;

            _logger.LogInformation(
                "Outgoing Response: {StatusCode} | Time Taken: {ElapsedMilliseconds} ms",
                response.StatusCode,
                stopwatch.ElapsedMilliseconds
            );
        }
    }
}
