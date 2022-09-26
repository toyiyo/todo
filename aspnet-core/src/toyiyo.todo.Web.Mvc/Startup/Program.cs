using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace toyiyo.todo.Web.Startup
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
                        o.Debug = true;
                        // Set TracesSampleRate to 1.0 to capture 100% of transactions for performance monitoring.
                        // We recommend adjusting this value in production.
                        o.TracesSampleRate = 1.0;
                    })
                .UseStartup<Startup>()
                .Build();
        }
    }
}
