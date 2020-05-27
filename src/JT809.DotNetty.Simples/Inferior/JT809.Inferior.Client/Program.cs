using JT809.DotNetty.Core;
using JT809.DotNetty.Core.Handlers;
using JT809.Protocol;
using JT809.Protocol.Configs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace JT809.Inferior.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serverHostBuilder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<ILoggerFactory, LoggerFactory>();
                    services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
                    services.AddSingleton(new JT809HeaderOptions
                    {
                        MsgGNSSCENTERID = 20141013,
                        Version = new JT809Header_Version(1, 0, 0),
                        EncryptKey = 9595
                    });
                    services.AddJT809Configure()
                            .AddJT809Core(hostContext.Configuration)
                            .AddJT809InferiorPlatformClient();
                    services.AddHostedService<JT809InferiorService>();
                });
            await serverHostBuilder.RunConsoleAsync();
        }
    }
}
