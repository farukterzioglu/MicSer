using System.Net.Http;
using System.Threading.Tasks;

namespace MicSer.BlockExplorer
{
    public class RpcProxy
    {
        private readonly HttpClient _httpClient;
        public RpcProxy(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task GenerateBlock()
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, "/")
            {
                Content = new StringContent("{\"jsonrpc\":\"1.0\",\"id\":\"curltext\",\"method\":\"generatetoaddress\",\"params\":[1,\"2MwsHLRMeBgkvb5cwwSMNC3qVDXpt7hX7qw\"]}")
            };
            var response = await _httpClient.SendAsync(requestMessage, default);
            response.EnsureSuccessStatusCode();
        }
    }
}