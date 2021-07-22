using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Windows.Forms;

namespace Firestar
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) => {
                    services.AddMemoryCache();
                    services.AddSingleton<CacheEngine>();
                });

            var host = builder.Build();

            using (var serviceScope = host.Services.CreateScope())
            {
                CacheEngine cacheEngine = serviceScope.ServiceProvider.GetRequiredService<CacheEngine>();

                System.Windows.Forms.Application.SetHighDpiMode(HighDpiMode.SystemAware);
                System.Windows.Forms.Application.EnableVisualStyles();
                System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
                System.Windows.Forms.Application.Run(new FirestarApplicationContext(cacheEngine));
            }
        }
    }
}
