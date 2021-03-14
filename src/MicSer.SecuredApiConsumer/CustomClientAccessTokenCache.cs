using System.Threading;
using System.Threading.Tasks;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MicSer.SecuredApiConsumer
{
    public class CustomClientAccessTokenCache : ClientAccessTokenCache, IClientAccessTokenCache
{
        public CustomClientAccessTokenCache(IDistributedCache cache, IOptions<AccessTokenManagementOptions> options, ILogger<ClientAccessTokenCache> logger) : base(cache, options, logger)
        {
        }

        Task IClientAccessTokenCache.SetAsync(string clientName, string accessToken, int expiresIn, ClientAccessTokenParameters parameters, CancellationToken cancellationToken) 
        { 
            return base.SetAsync(clientName, accessToken, expiresIn, parameters, cancellationToken);
        }

        async Task<ClientAccessToken> IClientAccessTokenCache.GetAsync(string clientName, ClientAccessTokenParameters parameters, CancellationToken cancellationToken)
        {
            var token = await base.GetAsync(clientName, parameters, cancellationToken);
            return token;
        }
    }
}