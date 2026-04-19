namespace InventorySystem.WebAPI.Options
{
    public class SecurityHardeningOptions
    {
        public bool RequireAuthenticatedByDefault { get; set; } = true;
        public int RequestsPerMinutePerIp { get; set; } = 120;
        public int IdempotencyTtlMinutes { get; set; } = 10;
        public string[] AllowedOrigins { get; set; } = new[] { "http://localhost:4200" };
    }
}
