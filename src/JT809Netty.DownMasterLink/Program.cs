using DotNetty.Handlers.Logging;
using JT809Netty.Core;
using JT809Netty.Core.Configs;
using JT809Netty.Core.Handlers;
using JT809Netty.Core.ServiceHandlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace JT809Netty.DownMasterLink
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serverHostBuilder = new HostBuilder()
                    .UseEnvironment(args[0].Split('=')[1])
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
                        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                              .AddJsonFile($"appsettings.{ hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
                    })
                    .ConfigureLogging((context, logging) =>
                    {
                        logging.AddConsole();
                        //NLog.LogManager.LoadConfiguration("Configs/nlog.config");
                        //logging.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
                        logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    })
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddSingleton<ILoggerFactory, LoggerFactory>();
                        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
                        services.Configure<JT809NettyOptions>(hostContext.Configuration.GetSection("JT809NettyOptions"));
                        services.AddSingleton<JT809BusinessTypeHandler, JT809BusinessTypeHandler>();
                        services.AddScoped<JT809DownMasterLinkConnectionHandler, JT809DownMasterLinkConnectionHandler>();
                        services.AddScoped<JT809DownMasterLinkServiceHandler, JT809DownMasterLinkServiceHandler>();
                        services.AddScoped<JT809DecodeHandler, JT809DecodeHandler>();
                        services.AddSingleton<IHostedService, JT809DownMasterLinkNettyService>();
                    });

            await serverHostBuilder.RunConsoleAsync();
        }
    }
}
