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
                .UseSentry()
                .UseStartup<Startup>()
                .Build();
        }
    }
}
