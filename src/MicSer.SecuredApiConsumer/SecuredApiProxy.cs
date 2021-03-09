using System.Net.Http;
using System.Threading.Tasks;

namespace MicSer.SecuredApiConsumer
{
    public class SecuredApiProxy
    {
        HttpClient _client;

        public SecuredApiProxy(HttpClient client)
        {
            _client = client;
        }

        public async Task<string> GetList()
        {
            var result = await _client.GetAsync("WeatherForecast");
            result.EnsureSuccessStatusCode();

            var response = await result.Content.ReadAsStringAsync();
            return response;
        }
    }
}