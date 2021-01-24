using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly.Registry;
using Serilog;

namespace MicSer.Explorer
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
                    services.AddHttpClient();

                    // services.AddHostedService<ExplorerJob>();
                    
                    PolicyRegistry registry = new PolicyRegistry();
                    registry.Add(ConsumerPolicies.GetTxCircuitBreaker, ConsumerPolicies.CreateGetTxCircuitBreaker());
                    services.AddSingleton<IReadOnlyPolicyRegistry<string>>(registry);

                    services.AddTransient<IHostedService, Consumer>();
                    services.AddTransient<IHostedService, Consumer>();
                });
    }
}
