using InventorySystem.WebAPI.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace InventorySystem.WebAPI.Middleware
{
    public class SimpleRateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDistributedCache _cache;
        private readonly SecurityHardeningOptions _options;

        public SimpleRateLimitMiddleware(RequestDelegate next, IDistributedCache cache, IOptions<SecurityHardeningOptions> options)
        {
            _next = next;
            _cache = cache;
            _options = options.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var minuteBucket = DateTime.UtcNow.ToString("yyyyMMddHHmm");
            var key = $"rl:{ip}:{minuteBucket}";

            var raw = await _cache.GetStringAsync(key);
            var current = string.IsNullOrEmpty(raw) ? 0 : int.Parse(raw, System.Globalization.CultureInfo.InvariantCulture);
            current++;
            if (current > _options.RequestsPerMinutePerIp)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    title = "Rate limit exceeded",
                    message = "Too many requests. Please retry shortly."
                });
                return;
            }

            await _cache.SetStringAsync(key, current.ToString(System.Globalization.CultureInfo.InvariantCulture), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
            });
            await _next(context);
        }
    }
}
