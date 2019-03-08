using DotNetty.Codecs.Http;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Libuv;
using JT809.DotNetty.Core.Configurations;
using JT809.DotNetty.Core.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace JT808.DotNetty.WebApi
{
    /// <summary>
    /// JT809 集成一个webapi服务
    /// </summary>
    internal class JT809MainWebAPIServerHost : IHostedService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<JT809MainWebAPIServerHost> logger;
        private DispatcherEventLoopGroup bossGroup;
        private WorkerEventLoopGroup workerGroup;
        private IChannel bootstrapChannel;
        private readonly JT809SuperiorPlatformOptions configuration;

        public JT809MainWebAPIServerHost(
            IServiceProvider provider,
            IOptions<JT809SuperiorPlatformOptions> jT809SuperiorPlatformOptionsAccessor,
            ILoggerFactory loggerFactory)
        {
            serviceProvider = provider;
            configuration = jT809SuperiorPlatformOptionsAccessor.Value;
            logger = loggerFactory.CreateLogger<JT809MainWebAPIServerHost>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            bossGroup = new DispatcherEventLoopGroup();
            workerGroup = new WorkerEventLoopGroup(bossGroup, 1);
            ServerBootstrap bootstrap = new ServerBootstrap();
            bootstrap.Group(bossGroup, workerGroup);
            bootstrap.Channel<TcpServerChannel>();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                bootstrap
                    .Option(ChannelOption.SoReuseport, true)
                    .ChildOption(ChannelOption.SoReuseaddr, true);
            }
            bootstrap
                    .Option(ChannelOption.SoBacklog, 8192)
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;
                        using (var scope = serviceProvider.CreateScope())
                        {
                            pipeline.AddLast("http_encoder", new HttpResponseEncoder());
                            pipeline.AddLast("http_decoder", new HttpRequestDecoder(4096, 8192, 8192, false));
                            //将多个消息转换为单一的request或者response对象 =>IFullHttpRequest
                            pipeline.AddLast("http_aggregator", new HttpObjectAggregator(65536));
                            pipeline.AddLast("http_jt809webapihandler", scope.ServiceProvider.GetRequiredService<JT809SuperiorWebAPIServerHandler>());
                        }
                    }));
            logger.LogInformation($"JT809 Superior WebAPI Server start at {IPAddress.Any}:{configuration.WebApiPort}.");
            return bootstrap.BindAsync(configuration.WebApiPort).ContinueWith(i => bootstrapChannel = i.Result);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await bootstrapChannel.CloseAsync();
            var quietPeriod = configuration.QuietPeriodTimeSpan;
            var shutdownTimeout = configuration.ShutdownTimeoutTimeSpan;
            await workerGroup.ShutdownGracefullyAsync(quietPeriod, shutdownTimeout);
            await bossGroup.ShutdownGracefullyAsync(quietPeriod, shutdownTimeout);
        }
    }
}
