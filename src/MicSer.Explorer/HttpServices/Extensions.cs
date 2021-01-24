using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace MicSer.Explorer.HttpServices
{
    public static class Extensions
    {
        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            Random jitterer = new Random();
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(6, retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + 
                    TimeSpan.FromMilliseconds(jitterer.Next(0, 100)));
        }
        
        public static IServiceCollection AddServicesForHttp(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHttpClient();
            serviceCollection.AddHttpClient<HttpProxy>(client =>
            {
                client.BaseAddress = new Uri("localhost");
            }).SetHandlerLifetime(TimeSpan.FromMinutes(5));

            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(10);
            serviceCollection.AddHttpClient("WithPolicies")
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(request => request.Method == HttpMethod.Get ? GetRetryPolicy() : Policy.NoOpAsync<HttpResponseMessage>())
                .AddPolicyHandler(timeoutPolicy)
                ;

            serviceCollection.AddScoped<BussinessClassScoped>();
            serviceCollection.AddSingleton<BussinessClassSingleton>();

            serviceCollection.AddHostedService<HttpWorker>();

            return serviceCollection;
        }
    }
}