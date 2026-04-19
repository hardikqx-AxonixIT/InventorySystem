using InventorySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.WebAPI.Middleware
{
    public class CommercialSubscriptionMiddleware
    {
        private readonly RequestDelegate _next;

        public CommercialSubscriptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext db, IConfiguration configuration)
        {
            if (!configuration.GetValue<bool>("Commercial:EnforceSubscription"))
            {
                await _next(context);
                return;
            }

            var path = context.Request.Path.Value ?? string.Empty;
            if (path.Contains("/api/commercial", StringComparison.OrdinalIgnoreCase) ||
                path.Contains("/health", StringComparison.OrdinalIgnoreCase) ||
                path.Contains("/swagger", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantValues))
            {
                await _next(context);
                return;
            }

            var tenantId = tenantValues.ToString();
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                await _next(context);
                return;
            }

            var sub = await db.TenantSubscriptions.AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId);
            if (sub == null || !sub.IsActive || sub.ExpiresAtUtc <= DateTime.UtcNow)
            {
                context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    title = "Subscription required",
                    tenantId,
                    message = "Active subscription not found for tenant."
                });
                return;
            }

            await _next(context);
        }
    }
}
