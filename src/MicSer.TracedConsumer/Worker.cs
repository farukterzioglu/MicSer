using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using OpenTelemetry.Trace;
using SomeFunctionsLibrary;

namespace MicSer.TracedConsumer
{
    public class Worker : BackgroundService
    {
        public static readonly ActivitySource ActivitySource = new ActivitySource("MicSer.TracedConsumer");
        
        private readonly ILogger<Worker> _logger;
        private readonly HttpClient _client;
        private readonly Helper _helper;

        public Worker(
            ILogger<Worker> logger, 
            IHttpClientFactory httpClientFactory, 
            Helper helper)
        {
            _logger = logger;
            _helper = helper;
            _client = httpClientFactory.CreateClient("MicSer.TracedApi");

            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            Activity.ForceDefaultIdFormat = true;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var activity = ActivitySource.StartActivity("ProcessLoop", ActivityKind.Server))
                {
                    await Task.Delay(1000, stoppingToken);
                    try
                    {
                        var response = await _client.GetAsync("/info", stoppingToken);
                        response.EnsureSuccessStatusCode();

                        var responseStr = await response.Content.ReadAsStringAsync(stoppingToken);
                        _logger.LogInformation($"Response: {responseStr}");

                        if(responseStr == "2")
                        {
                            using (var subActivity = ActivitySource.StartActivity("ProcessLoopSub", ActivityKind.Server))
                            {
                                await Task.Delay(TimeSpan.FromSeconds(1));
                            }
                        }

                        await _helper.DoSomething();
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogError(ex.ToString());
                        activity?.SetStatus(Status.Error.WithDescription(ex.ToString()));
                    }
                }
            }
        }
    }
}
