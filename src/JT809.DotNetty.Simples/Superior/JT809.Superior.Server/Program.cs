using JT809.DotNetty.Core;
using JT809.DotNetty.Core.Configurations;
using JT809.DotNetty.Core.Handlers;
using JT809.Protocol.Configs;
using JT809.Superior.Server.Configs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using JT809.KafkaService;
using JT809.Protocol;

namespace JT809.Superior.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {

            //5B0000005A02000000A81200013414CE010000000000270FD4C14131323334353600000000000000000000000002120200000024000E0407E316130F07EF4D80017018400032003300000096002D002D0000000300000000E08F5D
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
                            .AddJT809SuperiorPlatform(options:options => {
                                options.TcpPort = 809;
                            });
                    services.Configure<JT809GpsOptions>(hostContext.Configuration.GetSection("JT809GpsOptions"));
                    services.AddJT809KafkaProducerService(hostContext.Configuration);
                    services.Replace(new ServiceDescriptor(typeof(JT809SuperiorMsgIdReceiveHandlerBase), typeof(JT809SuperiorMsgIdReceiveHandler), ServiceLifetime.Singleton));
                });

            await serverHostBuilder.RunConsoleAsync();
        }
    }
}
