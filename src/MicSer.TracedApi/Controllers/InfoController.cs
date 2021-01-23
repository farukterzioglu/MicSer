using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MicSer.TracedApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class InfoController : ControllerBase
    {
        private readonly ILogger<InfoController> _logger;

        public InfoController(ILogger<InfoController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<int> Get()
        {
            if(new Random().Next(0,2) == 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(3));
                return 10;
            }

            return 2;
        }
    }
}
