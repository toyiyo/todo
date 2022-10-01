using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace toyiyo.todo.Web.Host.Startup
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseSentry(o =>
                    {
                        o.Dsn = "https://3623602af18444f4898645c07c46f003@o244776.ingest.sentry.io/6775629";
                        // When configuring for the first time, to see what the SDK is doing:
                        o.Debug = false;
                        // Set TracesSampleRate to 1.0 to capture 100% of transactions for performance monitoring.
                        // We recommend adjusting this value in production.
                        o.TracesSampleRate = 0.1;
                    })
                .UseStartup<Startup>()
                .Build();
        }
    }
}
