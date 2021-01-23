using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MicSer.TracedConsumer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly HttpClient _client;

        public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _client = httpClientFactory.CreateClient("MicSer.TracedApi");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);

                try
                {
                    var broadcastReponse = await _client.GetAsync("/info", stoppingToken);
                    broadcastReponse.EnsureSuccessStatusCode();

                    var broadcastReponseStr = await broadcastReponse.Content.ReadAsStringAsync(stoppingToken);
                    _logger.LogInformation($"Response: {broadcastReponseStr}");

                    if(broadcastReponseStr == "2")
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
            }
        }
    }
}
