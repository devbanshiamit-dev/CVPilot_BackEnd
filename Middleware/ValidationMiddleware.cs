using CVPilotAPI.Validate;

namespace CVPilotAPI.Middleware
{
    public class ValidationMiddleware
    {
        private readonly RequestDelegate _next;
        public ValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context, ValidateToken token)
        {
            // Validate the request here
            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrWhiteSpace(authHeader))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Authorization header missing.");
                return;
            }
            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid authorization header format.");
                return;
            }
            var accessToken = authHeader["Bearer ".Length..].Trim();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Access token missing.");
                return;
            }
            var principal = token.ValidateAccessToken(accessToken);
            if (null == principal)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid or expired access token.");
                return;
            }
            else
            {
                // Set the user principal in the HttpContext
                context.User = principal;
            }
            await _next(context);
        }
    }
}
