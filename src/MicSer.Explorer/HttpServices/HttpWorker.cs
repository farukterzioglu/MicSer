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
using Microsoft.Extensions.DependencyInjection;

namespace MicSer.Explorer.HttpServices
{
    public class HttpWorker : BackgroundService
    {
        private readonly ILogger<ExplorerJob> _logger;
        private readonly IServiceProvider _provider;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly BussinessClassSingleton _bussinessClass;

        public HttpWorker(
            ILogger<ExplorerJob> logger,
            IServiceScopeFactory serviceScopeFactory,
            BussinessClassSingleton bussinessClass
        )
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _bussinessClass = bussinessClass;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var bussinessClass = scope.ServiceProvider.GetService<BussinessClassScoped>();

                    await bussinessClass.DoSomething();
                }

                await _bussinessClass.DoSomething();
            }
        }
    }
}
