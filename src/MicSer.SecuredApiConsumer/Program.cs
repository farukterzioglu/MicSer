using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MicSer.SecuredApiConsumer
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
                    services.AddHostedService<Worker>();

                    services.AddClientAccessTokenManagement(options =>
                    {
                        options.Client.Clients.Add("SecuredApiProxy", new ClientCredentialsTokenRequest
                        {
                            Address = "http://localhost:8081/connect/token",
                            ClientId = "api-consumer",
                            ClientSecret = "e1844b86-6eb6-4ed8-95f3-aa3656672ede",
                            Scope = "list"
                        });
                        options.Client.CacheLifetimeBuffer = 10;
                    }).ConfigureBackchannelHttpClient(client =>
                    {
                        client.Timeout = TimeSpan.FromSeconds(30);
                    });
                    services.AddTransient<IClientAccessTokenCache, CustomClientAccessTokenCache>();
                    services.AddTransient<IClientAccessTokenManagementService, CustomClientAccessTokenManagementService>();

                    services.AddClientAccessTokenClient("securedApiClient", configureClient: client =>
                    {
                        client.BaseAddress = new Uri("http://localhost:6001/");
                    });

                    // or
                    services.AddHttpClient<SecuredApiProxy>(client =>
                    {
                        client.BaseAddress = new Uri("http://localhost:6001/");
                    })
                    .AddClientAccessTokenHandler("SecuredApiProxy");

                });
    }
}
