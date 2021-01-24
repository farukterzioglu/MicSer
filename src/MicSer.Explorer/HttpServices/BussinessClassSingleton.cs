using System.Net.Http;
using System.Threading.Tasks;

namespace MicSer.Explorer.HttpServices
{
    public class BussinessClassSingleton
    {
        private readonly IHttpClientFactory _factory;

        public BussinessClassSingleton(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        public async Task DoSomething()
        {
            using (var httpClient = _factory.CreateClient("BussinessClass"))
            {
                var result = await httpClient.GetAsync("localhost");
            }
        }
    }
}