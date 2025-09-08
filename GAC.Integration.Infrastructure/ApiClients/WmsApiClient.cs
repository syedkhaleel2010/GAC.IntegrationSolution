using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace GAC.Integration.Infrastructure.ApiClients
{
    public class WmsApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WmsApiClient> _logger;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

        public WmsApiClient(HttpClient httpClient, ILogger<WmsApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Retry up to 3 times with exponential backoff
            _retryPolicy = Policy<HttpResponseMessage>
                           .Handle<HttpRequestException>()
                           .OrResult(r => !r.IsSuccessStatusCode)
                           .WaitAndRetryAsync(
                               3,
                               attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                               (outcome, timespan, attempt, context) =>
                               {
                                   _logger.LogWarning("Retry {attempt} after {delay}s due to {reason}",
                                       attempt, timespan.TotalSeconds, outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString());
                               });
        }

        public async Task<HttpResponseMessage> PushPurchaseOrderAsync(object poDto)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _httpClient.PostAsJsonAsync("PurchaseOrder", poDto);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("PO successfully sent to WMS");
                    return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                }

                _logger.LogError("Failed to send PO to WMS. Status: {status}", response.StatusCode);
                return new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
            });
        }
    }
}
