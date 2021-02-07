using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MicSer.BlockExplorer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly RpcProxy _rpcProxy;

        public Worker(ILogger<Worker> logger, RpcProxy rpcProxy)
        {
            _logger = logger;
            _rpcProxy = rpcProxy;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(5000, stoppingToken);

                try
                {
                    await _rpcProxy.GenerateBlock();
                }
                catch (System.Exception ex)
                {
                     _logger.LogError(ex.ToString());
                }
            }
        }
    }
}
