using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MicSer.SecuredApiConsumer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                    try
                    {
                        var proxy = scope.ServiceProvider.GetService<SecuredApiProxy>();
                        var list = await proxy.GetList();
                    }
                    catch (System.Exception ex)
                    {
                         _logger.LogError(ex.ToString());
                    }
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }
    }
}
