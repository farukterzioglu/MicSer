using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MicSer.BlockExplorer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    RpcSettings rpcSettings =  new RpcSettings();
                    hostContext.Configuration.Bind("BitcoinGW", rpcSettings);

                    services.AddHostedService<Worker>();
                    services.AddHttpClient<RpcProxy>(cfg =>
                    {
                        cfg.BaseAddress = new Uri(rpcSettings.RpcUrl);
                        cfg.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                            "Basic", 
                            Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($"{rpcSettings.RpcUserName}:{rpcSettings.RpcPassword}")));
                        cfg.DefaultRequestHeaders.Add("Accept", "application/json");
                    });
                });
    }
}
