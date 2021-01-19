using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Fallback;
using Polly.Timeout;
using Polly.Wrap;
using Polly.CircuitBreaker;

namespace MicSer.Explorer
{
    public class NetworkInfo
    {
        public bool sync { get; set; }
    }

    public class BroadcastResult
    {
        public bool IsSuccess { get; set; }
        public string TxHash { get; set; }
    }

    public class ExplorerJob : BackgroundService
    {
        private readonly ILogger<ExplorerJob> _logger;
        private readonly HttpClient _client;

        public ExplorerJob(
            ILogger<ExplorerJob> logger, 
            HttpClient client)
        {
            _logger = logger;
            _client = client;

            _client.BaseAddress = new Uri("https://localhost:5001");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
                
                // var networkInfo = await QueryNetworkInfoButNotWaitMuch();
                // var networkInfo1 = await QueryNetworkInfoUntilGetSyncedButNotWaitMuch();
                // var networkInfo2 = await QueryNetworkInfoUntilGetSynced();

                // var balance = await GetBalanceWithRetryAndFallBack();
                // if(balance == 0) balance = await GetBalanceWithRetry();

                // await BroadcastWithTimeout(1);
                // await BroadcastWithTimeoutNonCancellable(1);
                // await BroadcastWithTimeoutButQuick(1);
                // await BroadcastWithPessimisticTimeoutWithCancellation();

                var fee = await GetFeeFastAndWithBackup(stoppingToken);
                _logger.LogInformation($"Fee: { fee }");
            }
        }

        private async Task BroadcastWithPessimisticTimeoutWithCancellation()
        {
            var timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(2), TimeoutStrategy.Pessimistic, (context, timespan, task) => 
            {
                task.ContinueWith(t => { // ContinueWith important!: the abandoned task may very well still be executing, when the caller times out on waiting for it! 
                    if (t.IsFaulted) 
                    {
                        _logger.LogError($"{context.PolicyKey} at {context.OperationKey}: execution timed out after {timespan.TotalSeconds} seconds, eventually terminated with: {t.Exception}.");
                    }
                    else if (t.IsCanceled)
                    {
                        // (If the executed delegates do not honour cancellation, this IsCanceled branch may never be hit.  It can be good practice however to include, in case a Policy configured with TimeoutStrategy.Pessimistic is used to execute a delegate honouring cancellation.)  
                        _logger.LogError($"{context.PolicyKey} at {context.OperationKey}: execution timed out after {timespan.TotalSeconds} seconds, task cancelled.");
                    }
                    else
                    {
                        // extra logic (if desired) for tasks which complete, despite the caller having 'walked away' earlier due to timeout.
                        _logger.LogCritical("Broadcasting completed succesfuuly.");
                    }

                    // Additionally, clean up any resources ...
                });

                return Task.CompletedTask;
            });
            
            AsyncFallbackPolicy<BroadcastResult> fallbackForTimeout = Policy<BroadcastResult>
                .Handle<TimeoutRejectedException>()
                .FallbackAsync(
                    fallbackValue: new BroadcastResult() { IsSuccess = false}, 
                    onFallbackAsync: async b =>
                    {
                        _logger.LogError("Broadcasting timed out!");
                    });

            AsyncFallbackPolicy<BroadcastResult> fallbackForAnyException = Policy<BroadcastResult>
                .Handle<Exception>()
                .FallbackAsync(
                    fallbackValue: new BroadcastResult() { IsSuccess = false},
                    onFallbackAsync: async e =>
                    {
                        _logger.LogError(e.ToString());
                    });

            AsyncPolicyWrap<BroadcastResult> policyWrap = fallbackForAnyException.WrapAsync(fallbackForTimeout).WrapAsync(timeoutPolicy);

            BroadcastResult result = await policyWrap.ExecuteAsync( async ct => {
                var broadcastReponse = await _client.PostAsync("/network/quickbroadcast", null, ct);
                broadcastReponse.EnsureSuccessStatusCode();

                var broadcastReponseStr = await broadcastReponse.Content.ReadAsStringAsync(ct);
                var broadcastResult = JsonSerializer.Deserialize<BroadcastResult>(broadcastReponseStr);
                return broadcastResult;
            }, CancellationToken.None);

            if(result.IsSuccess) _logger.LogInformation($"Transaction hash: {result}");
        }

        
        // Timeout is 2 second but we are not cancelling for critical jobs 
        private async Task BroadcastWithTimeoutButQuick(int txId)
        {
            var timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(2), TimeoutStrategy.Optimistic);
            
            AsyncFallbackPolicy<BroadcastResult> fallbackForTimeout = Policy<BroadcastResult>
                .Handle<TimeoutRejectedException>()
                .FallbackAsync(
                    fallbackValue: new BroadcastResult() { IsSuccess = false}, 
                    onFallbackAsync: async (DelegateResult<BroadcastResult> result) =>
                    {
                        _logger.LogError($"Result: {result.Result?.ToString()}, exception: {result.Exception?.ToString()}");
                    });

            AsyncFallbackPolicy<BroadcastResult> fallbackForAnyException = Policy<BroadcastResult>
                .Handle<Exception>()
                .FallbackAsync(
                    fallbackValue: new BroadcastResult() { IsSuccess = false},
                    onFallbackAsync: async (DelegateResult<BroadcastResult> result) =>
                    {
                        _logger.LogError($"Result: {result.Result?.ToString()}, exception: {result.Exception?.ToString()}");
                    });

            AsyncPolicyWrap<BroadcastResult> policyWrap = fallbackForAnyException.WrapAsync(fallbackForTimeout).WrapAsync(timeoutPolicy);

            BroadcastResult result = await policyWrap.ExecuteAsync( async cancellationToken => {
                // Some cancellable jobs...
                _logger.LogInformation("Starting to non-critical job...");
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                
                // Non cancellable jobs...
                _logger.LogInformation("Starting to critical job...");
                var broadcastReponse = await _client.PostAsync($"/network/quickbroadcast?txId={txId}", null, CancellationToken.None);
                broadcastReponse.EnsureSuccessStatusCode();

                return new BroadcastResult();
            }, CancellationToken.None);

            if(result.IsSuccess) _logger.LogInformation($"Transaction hash: {result}");
        }

        // Remote is accepting cancelleation token but it is quick, still return 200 even this metod times out
        private async Task BroadcastWithTimeoutButQuick1(int txId)
        {
            var timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(2), TimeoutStrategy.Optimistic);
            
            AsyncFallbackPolicy<BroadcastResult> fallbackForTimeout = Policy<BroadcastResult>
                .Handle<TimeoutRejectedException>()
                .FallbackAsync(
                    fallbackValue: new BroadcastResult() { IsSuccess = false}, 
                    onFallbackAsync: async b =>
                    {
                        _logger.LogError("Broadcasting timed out!");
                    });

            AsyncFallbackPolicy<BroadcastResult> fallbackForAnyException = Policy<BroadcastResult>
                .Handle<Exception>()
                .FallbackAsync(
                    fallbackValue: new BroadcastResult() { IsSuccess = false},
                    onFallbackAsync: async e =>
                    {
                        _logger.LogError(e.ToString());
                    });

            AsyncPolicyWrap<BroadcastResult> policyWrap = fallbackForAnyException.WrapAsync(fallbackForTimeout).WrapAsync(timeoutPolicy);

            BroadcastResult result = await policyWrap.ExecuteAsync( async cancellationToken => {
                var broadcastReponse = await _client.PostAsync($"/network/quickbroadcast?txId={txId}", null, cancellationToken);
                broadcastReponse.EnsureSuccessStatusCode();

                var broadcastReponseStr = await broadcastReponse.Content.ReadAsStringAsync(CancellationToken.None);
                var broadcastResult = JsonSerializer.Deserialize<BroadcastResult>(broadcastReponseStr);
                return broadcastResult;
            }, CancellationToken.None);

            if(result.IsSuccess) _logger.LogInformation($"Transaction hash: {result}");
        }

        // Remote is accepting cancelleation token and throw task cancelled.
        private async Task BroadcastWithTimeout(int txId)
        {
            var timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(2), TimeoutStrategy.Optimistic);
            
            AsyncFallbackPolicy<BroadcastResult> fallbackForTimeout = Policy<BroadcastResult>
                .Handle<TimeoutRejectedException>()
                .FallbackAsync(
                    fallbackValue: new BroadcastResult() { IsSuccess = false}, 
                    onFallbackAsync: async b =>
                    {
                        _logger.LogError("Broadcasting timed out!");
                    });

            AsyncFallbackPolicy<BroadcastResult> fallbackForAnyException = Policy<BroadcastResult>
                .Handle<Exception>()
                .FallbackAsync(
                    fallbackValue: new BroadcastResult() { IsSuccess = false},
                    onFallbackAsync: async e =>
                    {
                        _logger.LogError(e.ToString());
                    });

            AsyncPolicyWrap<BroadcastResult> policyWrap = fallbackForAnyException.WrapAsync(fallbackForTimeout).WrapAsync(timeoutPolicy);

            BroadcastResult result = await policyWrap.ExecuteAsync( async cancellationToken => {
                var broadcastReponse = await _client.PostAsync($"/network/quickbroadcastwithcancel?txId={txId}", null, cancellationToken);
                broadcastReponse.EnsureSuccessStatusCode();

                var broadcastReponseStr = await broadcastReponse.Content.ReadAsStringAsync(CancellationToken.None);
                var broadcastResult = JsonSerializer.Deserialize<BroadcastResult>(broadcastReponseStr);
                return broadcastResult;
            }, CancellationToken.None);

            if(result.IsSuccess) _logger.LogInformation($"Transaction hash: {result}");
        }

        // Remote is not accepting cancelleation token and returns 200, but http request is being cancelled and response is not being considered.
        private async Task BroadcastWithTimeoutNonCancellable(int txId)
        {
            var timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(2), TimeoutStrategy.Optimistic);
            
            AsyncFallbackPolicy<BroadcastResult> fallbackForTimeout = Policy<BroadcastResult>
                .Handle<TimeoutRejectedException>()
                .FallbackAsync(
                    fallbackValue: new BroadcastResult() { IsSuccess = false}, 
                    onFallbackAsync: async b =>
                    {
                        _logger.LogError("Broadcasting timed out!");
                    });

            AsyncFallbackPolicy<BroadcastResult> fallbackForAnyException = Policy<BroadcastResult>
                .Handle<Exception>()
                .FallbackAsync(
                    fallbackValue: new BroadcastResult() { IsSuccess = false},
                    onFallbackAsync: async (DelegateResult<BroadcastResult> result) =>
                    {
                        _logger.LogError($"Result: {result.Result?.ToString()}, exception: {result.Exception?.ToString()}");
                    });

            AsyncPolicyWrap<BroadcastResult> policyWrap = fallbackForAnyException.WrapAsync(fallbackForTimeout).WrapAsync(timeoutPolicy);

            BroadcastResult result = await policyWrap.ExecuteAsync( async cancellationToken => {
                var broadcastReponse = await _client.PostAsync($"/network/quickbroadcastwithoutcancel?txId={txId}", null, cancellationToken);
                broadcastReponse.EnsureSuccessStatusCode();

                var broadcastReponseStr = await broadcastReponse.Content.ReadAsStringAsync(CancellationToken.None);
                var broadcastResult = JsonSerializer.Deserialize<BroadcastResult>(broadcastReponseStr);
                return broadcastResult;
            }, CancellationToken.None);

            if(result.IsSuccess) _logger.LogInformation($"Transaction hash: {result}");
        }

        private async Task BroadcastWithBackup()
        {
            
        }

        private async Task BroadcastToAlternative()
        {
            
        }

        AsyncCircuitBreakerPolicy breakerPolicy = Policy
            .Handle<TimeoutRejectedException>()
            .Or<HttpRequestException>( e => e.StatusCode == HttpStatusCode.InternalServerError)
            .Or<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 3, 
                durationOfBreak: TimeSpan.FromSeconds(15)
            );

        private async Task<int> GetFeeFastAndWithBackup(CancellationToken cancellationToken)
        {
            int fallbackFee = 5;

            CircuitState breakerState = breakerPolicy.CircuitState;
            if (breakerState == CircuitState.Open ) 
            {
                _logger.LogWarning($"Circuit breaker is open, falling back to {fallbackFee}");
                return fallbackFee;
            }   

            var timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(4), TimeoutStrategy.Optimistic);

            AsyncFallbackPolicy<int> fallbackForTimeout = Policy<int>
                .Handle<TimeoutRejectedException>()
                .FallbackAsync<int>(
                    fallbackValue: fallbackFee, 
                    onFallbackAsync: async c => _logger.LogError($"Fee endpoint timed out, falling back to {fallbackFee}!")
                );

            AsyncFallbackPolicy<int> fallbackForException = Policy<int>
                .Handle<HttpRequestException>( e => e.StatusCode == HttpStatusCode.InternalServerError)
                .Or<BrokenCircuitException>()
                .Or<Exception>()
                .FallbackAsync<int>(
                    fallbackValue: fallbackFee, 
                    onFallbackAsync: async c => { 
                        var message = $"Couldn't get fee, falling back to {fallbackFee}! ";

                        switch (c.Exception)
                        {
                            case HttpRequestException httpRequestException:
                                message += $"Status code: {httpRequestException.StatusCode}";
                                break;
                            case BrokenCircuitException brokenCircuitException:
                                message += $"Exception: {brokenCircuitException.InnerException.Message}";
                                break;
                            default:
                                message += $"Exception: {c.Exception.Message}";
                                break;
                        }

                        _logger.LogError(message); 
                    });
            
            AsyncPolicyWrap<int> policyWrap = 
                fallbackForException.WrapAsync(
                    fallbackForTimeout).WrapAsync(
                        breakerPolicy).WrapAsync(
                            timeoutPolicy);

            int result = await policyWrap.ExecuteAsync( async cancellationToken => {
                var feeReponse = await _client.GetAsync($"/network/getfee", cancellationToken);
                feeReponse.EnsureSuccessStatusCode();

                var feeReponseStr = await feeReponse.Content.ReadAsStringAsync(CancellationToken.None);
                return int.Parse(feeReponseStr);
            }, cancellationToken);

            return result;
        }

        private async Task<int> GetBalanceWithRetry()
        {
            var policy = Policy
                .Handle<HttpRequestException>( e => e.StatusCode == HttpStatusCode.GatewayTimeout)
                .Or<HttpRequestException>( e => e.StatusCode == HttpStatusCode.InternalServerError)
                .WaitAndRetryAsync(
                    retryCount: 5, 
                    sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(500),
                    onRetry: (exception, calculatedWaitDuration) => { 
                        _logger.LogError($"Couldn't get balance, retrying. Exception: {exception.ToString()}"); 
                    });

            try
            {
                return await policy.ExecuteAsync(async token => {
                    var broadcastReponse = await _client.GetAsync("/network/unstablebalance");
                    broadcastReponse.EnsureSuccessStatusCode();

                    var broadcastReponseStr = await broadcastReponse.Content.ReadAsStringAsync();
                    var broadcastResult = JsonSerializer.Deserialize<int>(broadcastReponseStr);
                    return broadcastResult;
                }, CancellationToken.None);
            }
            catch (System.Exception ex)
            {
                // Instead we could define a fallback policy that return 0, kept this as sample
                _logger.LogError(ex.ToString());
                return 0;
            }
        }

        // Retries one time if it gets GatewayTimeout or InternalServerError, if it still can't get response, fallbacks to 0 balance 
        private async Task<int> GetBalanceWithRetryAndFallBack()
        {
            var retrypolicy = Policy
                .Handle<HttpRequestException>( e => e.StatusCode == HttpStatusCode.GatewayTimeout)
                .Or<HttpRequestException>( e => e.StatusCode == HttpStatusCode.InternalServerError)
                .WaitAndRetryAsync(
                    retryCount: 1, 
                    sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(500),
                    onRetry: (exception, calculatedWaitDuration) => { 
                        _logger.LogError($"Couldn't get balance, retrying. Exception: {exception.ToString()}"); 
                    });

            AsyncFallbackPolicy<int> fallbackForRetry = Policy<int>
                .Handle<Exception>()
                .FallbackAsync( fallbackValue: 0, onFallbackAsync: async b => 
                    _logger.LogError("Couldn't get balance!")
                );

            AsyncPolicyWrap<int> policyWrap = fallbackForRetry.WrapAsync(retrypolicy);

            return await policyWrap.ExecuteAsync(async token => {
                var broadcastReponse = await _client.GetAsync("/network/balance");
                broadcastReponse.EnsureSuccessStatusCode();

                var broadcastReponseStr = await broadcastReponse.Content.ReadAsStringAsync();
                var broadcastResult = JsonSerializer.Deserialize<int>(broadcastReponseStr);
                return broadcastResult;
            }, CancellationToken.None);
        }


        private async Task<NetworkInfo> QueryNetworkInfoUntilGetSynced()
        {
            var retrypolicy = Policy.Handle<ApplicationException>().WaitAndRetryAsync(
                retryCount: 20,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(1 * Math.Pow(2, attempt)), 
                onRetry: (exception, calculatedWaitDuration) => { 
                    _logger.LogWarning($"Network is not synced yet, trying {calculatedWaitDuration} later...");
                });

            var exceptionPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(200),
                onRetry: (exception, calculatedWaitDuration) => { 
                    _logger.LogError($"Couldn't get network info, trying {calculatedWaitDuration} later...");
                });

            AsyncPolicyWrap policyWrap = exceptionPolicy.WrapAsync(retrypolicy);

            return await policyWrap.ExecuteAsync<NetworkInfo>(async token =>
            {
                var networkInfoReponse = await _client.GetAsync("/network/info");
                networkInfoReponse.EnsureSuccessStatusCode();

                var networkInfoStr = await networkInfoReponse.Content.ReadAsStringAsync();
                var networkInfo = JsonSerializer.Deserialize<NetworkInfo>(networkInfoStr);

                if(!networkInfo.sync) 
                    throw new ApplicationException("Network is not synced yet!");

                return networkInfo;
            }, CancellationToken.None);
        }

        // Try until network get synced, but don't wait more than 10 seconds 
        private async Task<NetworkInfo> QueryNetworkInfoUntilGetSyncedButNotWaitMuch()
        {
            var timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(10), TimeoutStrategy.Optimistic);

            var retrypolicy = Policy.Handle<ApplicationException>().WaitAndRetryForeverAsync(
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(3), 
                onRetry: (exception, calculatedWaitDuration) => { 
                    _logger.LogWarning($"Network is not synced yet, trying {calculatedWaitDuration} later...");
                });

            AsyncFallbackPolicy<NetworkInfo> fallback = Policy<NetworkInfo>
                .Handle<TimeoutRejectedException>()
                .FallbackAsync( 
                    fallbackValue: new NetworkInfo(){ sync = false }, 
                    onFallbackAsync: async b => {
                        _logger.LogError("Network is probably down!");
                    });

            AsyncPolicyWrap<NetworkInfo> policyWrap = 
                fallback.WrapAsync(
                    timeoutPolicy).WrapAsync(
                        retrypolicy);

            return await policyWrap.ExecuteAsync(async token =>
            {
                var networkInfoReponse = await _client.GetAsync("/network/infodown");
                networkInfoReponse.EnsureSuccessStatusCode();

                var networkInfoStr = await networkInfoReponse.Content.ReadAsStringAsync();
                var networkInfo = JsonSerializer.Deserialize<NetworkInfo>(networkInfoStr);

                if(!networkInfo.sync) 
                    throw new ApplicationException("Network is not synced yet!");

                return networkInfo;
            }, CancellationToken.None);
        }

        // Waits only 3 seconds for every request, if times out, retries 3 times
        private async Task<NetworkInfo> QueryNetworkInfoButNotWaitMuch()
        {
            var retrypolicy = Policy.Handle<TimeoutRejectedException>().WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(1), 
                onRetry: (exception, calculatedWaitDuration) => { 
                    _logger.LogWarning($"Couldn't get response in 3 sec, trying {calculatedWaitDuration} later...");
                });

            var timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(3), TimeoutStrategy.Optimistic);

            AsyncFallbackPolicy<NetworkInfo> fallback = Policy<NetworkInfo>
                .Handle<TimeoutRejectedException>()
                .FallbackAsync( 
                    fallbackValue: new NetworkInfo(){ sync = false }, 
                    onFallbackAsync: async b => {
                        _logger.LogError("Network is probably down or too slow!");
                    });

            AsyncPolicyWrap<NetworkInfo> policyWrap = 
                fallback.WrapAsync(
                    retrypolicy).WrapAsync(
                        timeoutPolicy);

            return await policyWrap.ExecuteAsync(async token =>
            {
                var networkInfoReponse = await _client.GetAsync("/network/infoslow", token);
                var networkInfoStr = await networkInfoReponse.Content.ReadAsStringAsync(token);
                var networkInfo = JsonSerializer.Deserialize<NetworkInfo>(networkInfoStr);

                return networkInfo;
            }, CancellationToken.None);
        }

        private async Task<NetworkInfo> GetTxFromRateLimitiedApi()
        {
            // fails after 3 requests in 5 seconds.
            throw new NotImplementedException();
        }
    }
}
