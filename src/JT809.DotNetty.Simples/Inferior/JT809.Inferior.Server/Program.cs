using JT809.DotNetty.Core;
using JT809.DotNetty.Core.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using JT809.Protocol.Extensions.DependencyInjection;
using JT809.Protocol.Extensions.DependencyInjection.Options;

namespace JT809.Inferior.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {

            //作为从链路服务器，接收上级平台连接请求包括
            //1.从链路连接请求消息-从链路连接应答消息
            //5B 00 00 00 1B 00 00 00 02 90 02 00 01 87 72 01 00 00 00 00 00 00 00 00 AB 10 5D
            //2.从链路连接保持请求消息-从链路连接保持应答消息
            //5B 00 00 00 1A 00 00 00 03 90 06 00 01 87 72 01 00 00 00 00 00 00 00 64 E7 5D
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
                    services.AddJT809Configure(new JT809Options
                    {
                         HeaderOptions=new Protocol.Configs.JT809HeaderOptions
                         {
                              MsgGNSSCENTERID=100210
                         }
                    });
                    services.AddJT809Core(hostContext.Configuration)
                            .AddJT809InferiorPlatform(options: options => {
                                options.TcpPort = 809;
                            });
                });
            await serverHostBuilder.RunConsoleAsync();
        }
    }
}
