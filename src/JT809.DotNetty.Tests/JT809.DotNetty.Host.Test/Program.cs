using JT809.DotNetty.Core;
using JT809.DotNetty.Tcp;
using JT809.Protocol.Configs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace JT809.DotNetty.Host.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            JT809.Protocol.JT809GlobalConfig.Instance
                .SetHeaderOptions(new JT809HeaderOptions
                {
                    MsgGNSSCENTERID = 20190222,
                    Version = new JT809.Protocol.JT809Header_Version(1, 0, 0),
                    EncryptKey = 9595
                });

            //主链路登录请求消息
            //5B00000048000000851001013353D5010000000000270F0133530D32303134303831333132372E302E302E3100000000000000000000000000000000000000000000001FA3275F5D
            //主链路注销请求消息
            //5B000000260000008510030134140E010000000000270F0001E24031323334353600003FE15D
            //主链路连接保持请求消息
            //5B0000001A0000008510050134140E010000000000270FBA415D

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
                    services.AddJT809Core(hostContext.Configuration)
                            .AddJT809TcpHost();
                });

            await serverHostBuilder.RunConsoleAsync();
        }
    }
}
