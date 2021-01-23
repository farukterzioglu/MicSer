using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SomeFunctionsLibrary
{
    public class Helper
    {
        public static readonly ActivitySource ActivitySource = new ActivitySource("SomeFunctionsLibrary");

        public async Task DoSomething()
        {
            var activity = ActivitySource
                .StartActivity("DoneSomething", kind: ActivityKind.Server)
                ?
                .SetStartTime(DateTime.UtcNow)
                .AddTag("Category", "tracing");

            // Doing some critical things...
            await Task.Delay(TimeSpan.FromMilliseconds(200));

            activity
                ?
                .AddEvent(new ActivityEvent("done-something"))
                .AddBaggage("Information", "some-info");

            activity
                ?
                .Stop();
        }
    }
}