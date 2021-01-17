using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Threading;

namespace MicSer.Api.Controllers
{
    public class NetworkInfo
    {
        public bool sync { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class NetworkController : ControllerBase
    {
        private readonly ILogger<NetworkController> _logger;

        public NetworkController(ILogger<NetworkController> logger)
        {
            _logger = logger;
        }

        private static int networkInfoCounter = 0;
        [HttpGet("info")]
        public async Task<IActionResult> Info()
        {
            if(networkInfoCounter++ == 0) 
            {
                throw new Exception();
            }

            _logger.LogInformation($"networkInfoCounter: {networkInfoCounter}");

            if(networkInfoCounter == 5) 
            {
                networkInfoCounter = 0;
                return Ok(new NetworkInfo{ sync = true });
            }

            return Ok(new NetworkInfo{ sync = false });
        }

        [HttpGet("infoslow")]
        public async Task<IActionResult> InfoSlow(CancellationToken token)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), token);
            return Ok(new NetworkInfo{ sync = true });
        }

        [HttpGet("infodown")]
        public IActionResult InfoDown()
        {
            return Ok(new NetworkInfo{ sync = false });
        }

        private static int counter = 0;
        [HttpPost("broadcast")]
        public IActionResult Broadcast()
        {
            if(++counter == 5)
            {
                counter = 0;
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            
            return Ok("0x6b59a553e467bcbbf668021f59268d9aeeebca3cfbe1d4a5e1475521896837f8");
        }

        [HttpPost("broadcastbackup")]
        public IActionResult BroadcastBackup()
        {
            return Ok("0x6b59a553e467bcbbf668021f59268d9aeeebca3cfbe1d4a5e1475521896837f8");
        }

        [HttpPost("quickbroadcast")]
        public async Task<IActionResult> QuickBroadcast(CancellationToken token)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500), token);

            // Processing the tx... (this is non cancellable)
            await Task.Delay(TimeSpan.FromSeconds(3), CancellationToken.None);
            return Ok("0x6b59a553e467bcbbf668021f59268d9aeeebca3cfbe1d4a5e1475521896837f8");
        }

        [HttpPost("quickbroadcastwithcancel")]
        public async Task<IActionResult> QuickBroadcastWithCancellation(CancellationToken token)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), token);
            return Ok("0x6b59a553e467bcbbf668021f59268d9aeeebca3cfbe1d4a5e1475521896837f8");
        }

        [HttpPost("quickbroadcastwithoutcancel")]
        public async Task<IActionResult> QuickBroadcastWithoutCancellation()
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            return Ok("0x6b59a553e467bcbbf668021f59268d9aeeebca3cfbe1d4a5e1475521896837f8");
        }

        private static ushort balanceRetry = 0;
        [HttpGet("balance")]
        public IActionResult Balance()
        {
            if(++balanceRetry == 3)
            {
                _logger.LogInformation("Balance is 100");
                balanceRetry = 0;
                return Ok(100);
            }
            
            _logger.LogWarning("Internal server error!");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        private static ushort unstableBalanceRetry = 0;
        [HttpGet("unstablebalance")]
        public IActionResult UnstableBalance()
        {
            switch(++unstableBalanceRetry)
            {
                case 1:
                    _logger.LogWarning("Timed out!");
                    return StatusCode(StatusCodes.Status504GatewayTimeout);
                case 2: 
                    _logger.LogWarning("Internal server error!");
                    return StatusCode(StatusCodes.Status500InternalServerError);
                case 3:
                    _logger.LogInformation("Balance is 100");
                    unstableBalanceRetry = 0;
                    return Ok(100);
                default:
                    return StatusCode(StatusCodes.Status400BadRequest);
            }
        }

        [HttpGet("tx")]
        public IActionResult GetTx()
        {
            // fail after 3 requests in 5 seconds.
            throw new NotImplementedException();
        }
    }
}
