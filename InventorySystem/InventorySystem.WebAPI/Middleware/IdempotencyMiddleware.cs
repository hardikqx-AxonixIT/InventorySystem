using InventorySystem.WebAPI.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace InventorySystem.WebAPI.Middleware
{
    public class IdempotencyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDistributedCache _cache;
        private readonly SecurityHardeningOptions _options;

        public IdempotencyMiddleware(RequestDelegate next, IDistributedCache cache, IOptions<SecurityHardeningOptions> options)
        {
            _next = next;
            _cache = cache;
            _options = options.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!(HttpMethods.IsPost(context.Request.Method) || HttpMethods.IsPut(context.Request.Method) || HttpMethods.IsPatch(context.Request.Method)))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue("X-Idempotency-Key", out var idempotencyKey) || string.IsNullOrWhiteSpace(idempotencyKey))
            {
                await _next(context);
                return;
            }

            var key = $"idem:{context.Request.Method}:{context.Request.Path}:{idempotencyKey}";
            if (await _cache.GetStringAsync(key) != null)
            {
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                await context.Response.WriteAsJsonAsync(new
                {
                    title = "Duplicate request",
                    message = "This request has already been processed recently."
                });
                return;
            }

            await _cache.SetStringAsync(key, "1", new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.IdempotencyTtlMinutes)
            });
            await _next(context);
        }
    }
}
