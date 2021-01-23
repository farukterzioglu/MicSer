using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using SomeFunctionsLibrary;

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
                    services.AddTransient<Helper>();
                    services.AddHostedService<Worker>();

                    services.AddHttpClient("MicSer.TracedApi", cfg =>
                    {
                        cfg.BaseAddress = new Uri("http://localhost:5005");
                        cfg.DefaultRequestHeaders.Add("Accept", "application/json");
                        cfg.DefaultRequestHeaders.Add("User-Agent", "MicSer.TracedConsumer");
                    });

                    ConfigureOpenTelemetry(hostContext.Configuration, services);
                });

        private static void ConfigureOpenTelemetry(IConfiguration configuration, IServiceCollection services)
        {
            services.AddOpenTelemetryTracing((builder) => builder
                .AddSource("MicSer.TracedConsumer")
                .AddSource("SomeFunctionsLibrary")
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(
                    serviceName: "TracedConsumer",
                    serviceNamespace:  "MicSer" ))
                // .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation(
                    (options) => options.Filter =
                        (httpRequestMessage) =>
                        {
                            return 
                                !httpRequestMessage.RequestUri.Host.Contains("6e7") ; // 3. party logger requests
                        })
                // .AddConsoleExporter()
                .AddJaegerExporter()
                .AddNewRelicExporter(options =>
                {
                    options.ApiKey = configuration.GetValue<string>("NewRelic:ApiKey");
                    options.Endpoint = new Uri("https://trace-api.eu.newrelic.com/trace/v1");
                })
            );
        }
    }
}
