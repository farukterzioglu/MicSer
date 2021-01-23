using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static readonly ActivitySource ActivitySource = new ActivitySource("MicSer.TracedApi");
        
        private readonly ILogger<InfoController> _logger;

        public InfoController(ILogger<InfoController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<int> Get()
        {
            using (var activity = ActivitySource.StartActivity("ResponseCreation", ActivityKind.Server))
            {
                if(new Random().Next(0,2) == 0)
                {
                    using (var subActivity = ActivitySource.StartActivity("LongRunningTask", ActivityKind.Server))
                    {
                        await Task.Delay(TimeSpan.FromSeconds(3));
                    }

                    using (var subActivity = ActivitySource.StartActivity("ShortRunningTask", ActivityKind.Server))
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500));
                    }
                    
                    return 10;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500));
                return 2;
            }
        }
    }
}
