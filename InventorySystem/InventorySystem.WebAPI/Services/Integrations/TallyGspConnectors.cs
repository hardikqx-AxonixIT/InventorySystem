using System.Net.Http.Json;
using System.Text.Json;
using InventorySystem.Application.Services;
using InventorySystem.WebAPI.Options;
using Microsoft.Extensions.Options;

namespace InventorySystem.WebAPI.Services.Integrations
{
    public interface ITallyIntegrationConnector
    {
        Task<object> ImportMastersAsync(CancellationToken cancellationToken = default);

        Task<object> SyncLedgersVouchersAsync(CancellationToken cancellationToken = default);
    }

    public interface IGspGstConnector
    {
        Task<object> FileGstAsync(GstrExportFilterDto request, CancellationToken cancellationToken = default);
    }

    public sealed class TallyIntegrationConnector : ITallyIntegrationConnector
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IntegrationFeatureFlagsOptions _flags;

        public TallyIntegrationConnector(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IOptions<IntegrationFeatureFlagsOptions> flags)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _flags = flags.Value;
        }

        public async Task<object> ImportMastersAsync(CancellationToken cancellationToken = default)
        {
            var endpoint = _configuration["Integrations:Tally:Endpoint"];
            if (_flags.TallyConnectorEnabled && !string.IsNullOrWhiteSpace(endpoint))
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(endpoint.TrimEnd('/') + "/");
                var response = await client.PostAsJsonAsync("import-masters", new { }, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                return new
                {
                    success = response.IsSuccessStatusCode,
                    mode = "connector",
                    statusCode = (int)response.StatusCode,
                    body = TryParseJson(body)
                };
            }

            return new
            {
                success = !string.IsNullOrWhiteSpace(endpoint),
                mode = "placeholder",
                featureEnabled = _flags.TallyConnectorEnabled,
                note = string.IsNullOrWhiteSpace(endpoint)
                    ? "Tally endpoint not configured. Set Integrations:Tally:Endpoint and enable Integrations:FeatureFlags:TallyConnectorEnabled."
                    : $"Tally connector ready but feature flag off. Endpoint: {endpoint}"
            };
        }

        public async Task<object> SyncLedgersVouchersAsync(CancellationToken cancellationToken = default)
        {
            var endpoint = _configuration["Integrations:Tally:Endpoint"];
            if (_flags.TallyConnectorEnabled && !string.IsNullOrWhiteSpace(endpoint))
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(endpoint.TrimEnd('/') + "/");
                var response = await client.PostAsJsonAsync("sync-ledgers-vouchers", new { }, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                return new
                {
                    success = response.IsSuccessStatusCode,
                    mode = "connector",
                    statusCode = (int)response.StatusCode,
                    body = TryParseJson(body)
                };
            }

            return new
            {
                success = !string.IsNullOrWhiteSpace(endpoint),
                mode = "placeholder",
                featureEnabled = _flags.TallyConnectorEnabled,
                note = string.IsNullOrWhiteSpace(endpoint)
                    ? "Tally endpoint not configured. Set Integrations:Tally:Endpoint and enable Integrations:FeatureFlags:TallyConnectorEnabled."
                    : $"Tally sync ready but feature flag off. Endpoint: {endpoint}"
            };
        }

        private static object? TryParseJson(string raw)
        {
            try
            {
                return JsonSerializer.Deserialize<JsonElement>(raw);
            }
            catch
            {
                return raw;
            }
        }
    }

    public sealed class GspGstConnector : IGspGstConnector
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IntegrationFeatureFlagsOptions _flags;

        public GspGstConnector(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IOptions<IntegrationFeatureFlagsOptions> flags)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _flags = flags.Value;
        }

        public async Task<object> FileGstAsync(GstrExportFilterDto request, CancellationToken cancellationToken = default)
        {
            var gspEndpoint = _configuration["Integrations:Gsp:Endpoint"];
            if (_flags.GspConnectorEnabled && !string.IsNullOrWhiteSpace(gspEndpoint))
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(gspEndpoint.TrimEnd('/') + "/");
                var response = await client.PostAsJsonAsync("file", request, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                return new
                {
                    success = response.IsSuccessStatusCode,
                    mode = "connector",
                    statusCode = (int)response.StatusCode,
                    fromDate = request.FromDate,
                    toDate = request.ToDate,
                    body = TryParseJson(body)
                };
            }

            return new
            {
                success = !string.IsNullOrWhiteSpace(gspEndpoint),
                mode = "placeholder",
                featureEnabled = _flags.GspConnectorEnabled,
                fromDate = request.FromDate,
                toDate = request.ToDate,
                note = string.IsNullOrWhiteSpace(gspEndpoint)
                    ? "GSP endpoint not configured. Set Integrations:Gsp:Endpoint and enable Integrations:FeatureFlags:GspConnectorEnabled."
                    : $"GSP filing ready but feature flag off. Endpoint: {gspEndpoint}"
            };
        }

        private static object? TryParseJson(string raw)
        {
            try
            {
                return JsonSerializer.Deserialize<JsonElement>(raw);
            }
            catch
            {
                return raw;
            }
        }
    }
}
