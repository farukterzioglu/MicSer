using System.Net.Http;
using System.Threading.Tasks;

namespace MicSer.Explorer.HttpServices
{
    public class HttpProxy
    {
        private readonly HttpClient _httpClient;

        public HttpProxy(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetSomething()
        {
            var result = await _httpClient.GetAsync("/getInfo");
            return await result.Content.ReadAsStringAsync();
        }
    }
}