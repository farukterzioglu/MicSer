using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MicSer.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NetworkController : ControllerBase
    {
        private readonly ILogger<NetworkController> _logger;

        public NetworkController(ILogger<NetworkController> logger)
        {
            _logger = logger;
        }

        [HttpGet("info")]
        public async Task<IActionResult> Info()
        {
            await Task.Delay(TimeSpan.FromSeconds(15));
            return Ok();
        }
    }
}
