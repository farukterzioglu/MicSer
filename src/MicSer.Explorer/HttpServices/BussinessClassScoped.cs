using System.Net.Http;
using System.Threading.Tasks;

namespace MicSer.Explorer.HttpServices
{
    public class BussinessClassScoped
    {
        private readonly HttpProxy _httpProxy;

        public BussinessClassScoped(HttpProxy httpProxy)
        {
            _httpProxy = httpProxy;
        }

        public async Task DoSomething()
        {
            var result = await _httpProxy.GetSomething();
        }
    }
}