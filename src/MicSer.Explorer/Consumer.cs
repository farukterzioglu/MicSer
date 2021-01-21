using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Registry;
using Polly.Wrap;

namespace MicSer.Explorer
{
    public static class ConsumerPolicies
    {
        public static string GetTxCircuitBreaker = nameof(GetTxCircuitBreaker);
        
        /// <summary>
        /// Return a circuit breaker policy with config: 3 exceptions in 30 sec
        /// </summary>
        /// <returns></returns>
        public static ICircuitBreakerPolicy CreateGetTxCircuitBreaker()
        {
            AsyncCircuitBreakerPolicy breakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 3, 
                    durationOfBreak: TimeSpan.FromSeconds(15)
                );

            return breakerPolicy;
        } 
    }

    public class Consumer : BackgroundService
    {
        public static int counsumerCount = 0;

        private readonly ILogger _logger;
        private readonly HttpClient _client;
        private readonly IReadOnlyPolicyRegistry<string> _registry;

        public Consumer(
            ILoggerFactory loggerFactory, 
            HttpClient client,
            IReadOnlyPolicyRegistry<string> registry)
        {
            _logger = loggerFactory.CreateLogger($"Consumer {++counsumerCount}");
            _client = client;
            _registry = registry;

            _client.BaseAddress = new Uri("https://localhost:5001");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);

                var policy = _registry.Get<AsyncCircuitBreakerPolicy>(ConsumerPolicies.GetTxCircuitBreaker);
                if(policy.CircuitState == CircuitState.Open)
                {
                    _logger.LogWarning("Circuit breaker is open...");
                     continue;
                }

                AsyncFallbackPolicy<string> fallbackForCircuitBreaker = Policy<string>
                    .Handle<Exception>()
                    .FallbackAsync<string>(
                        fallbackValue: string.Empty,
                        onFallbackAsync: async c => _logger.LogError($"{ c.Exception.Message}")
                    );

                AsyncPolicyWrap<string> policyWrap = fallbackForCircuitBreaker.WrapAsync(policy);

                var txHash = await policyWrap.ExecuteAsync( async (cancellationToken) => {
                    var feeReponse = await _client.GetAsync($"/network/tx", cancellationToken);
                    feeReponse.EnsureSuccessStatusCode();

                    _logger.LogInformation("Got the tx.");
                    return "txhash";
                }, stoppingToken);

                if(string.IsNullOrEmpty(txHash)) continue;
            }
        }
    }
}