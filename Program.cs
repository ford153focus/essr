using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace essr
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Chromium.GetInstance(); // initialize singleton before everything
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        }
    }
}
