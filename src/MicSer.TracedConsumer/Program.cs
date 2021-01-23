using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace MicSer.TracedConsumer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((hostingContext, loggerConfiguration) =>
                {
                    loggerConfiguration
                        .Enrich.FromLogContext()
                        .ReadFrom.Configuration(hostingContext.Configuration);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();

                    services.AddHttpClient("MicSer.TracedApi", cfg =>
                    {
                        cfg.BaseAddress = new Uri("http://localhost:5005");
                        cfg.DefaultRequestHeaders.Add("Accept", "application/json");
                        cfg.DefaultRequestHeaders.Add("User-Agent", "MicSer.TracedConsumer");
                    });
                });
    }
}
