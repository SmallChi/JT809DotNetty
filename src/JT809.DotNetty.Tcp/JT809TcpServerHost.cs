using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Libuv;
using JT809.DotNetty.Core.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using JT809.Protocol;
using JT809.DotNetty.Core.Codecs;
using JT809.DotNetty.Tcp.Handlers;

namespace JT809.DotNetty.Tcp
{
    /// <summary>
    /// JT809 Tcp网关服务
    /// </summary>
    internal class JT809TcpServerHost : IHostedService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly JT809Configuration configuration;
        private readonly ILogger<JT809TcpServerHost> logger;
        private DispatcherEventLoopGroup bossGroup;
        private WorkerEventLoopGroup workerGroup;
        private IChannel bootstrapChannel;
        private IByteBufferAllocator serverBufferAllocator;

        public JT809TcpServerHost(
            IServiceProvider provider,
            ILoggerFactory loggerFactory,
            IOptions<JT809Configuration> jT809ConfigurationAccessor)
        {
            serviceProvider = provider;
            configuration = jT809ConfigurationAccessor.Value;
            logger=loggerFactory.CreateLogger<JT809TcpServerHost>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            bossGroup = new DispatcherEventLoopGroup();
            workerGroup = new WorkerEventLoopGroup(bossGroup, configuration.EventLoopCount);
            serverBufferAllocator = new PooledByteBufferAllocator();
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
               .Option(ChannelOption.SoBacklog, configuration.SoBacklog)
               .ChildOption(ChannelOption.Allocator, serverBufferAllocator)
               .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
               {
                   IChannelPipeline pipeline = channel.Pipeline;
                   using (var scope = serviceProvider.CreateScope())
                   {
                       channel.Pipeline.AddLast("jt809SystemIdleState", new IdleStateHandler(
                                                configuration.ReaderIdleTimeSeconds,
                                                configuration.WriterIdleTimeSeconds,
                                                configuration.AllIdleTimeSeconds));
                       channel.Pipeline.AddLast("jt809TcpConnection", scope.ServiceProvider.GetRequiredService<JT809TcpConnectionHandler>());
                       channel.Pipeline.AddLast("jt809TcpBuffer", new DelimiterBasedFrameDecoder(int.MaxValue,
                           Unpooled.CopiedBuffer(new byte[] { JT809Package.BEGINFLAG }),
                           Unpooled.CopiedBuffer(new byte[] { JT809Package.ENDFLAG })));
                       channel.Pipeline.AddLast("jt809TcpDecode", scope.ServiceProvider.GetRequiredService<JT809TcpDecoder>());
                       channel.Pipeline.AddLast("jt809TcpService", scope.ServiceProvider.GetRequiredService<JT809TcpServerHandler>());
                   }
               }));
            logger.LogInformation($"JT809 TCP Server start at {IPAddress.Any}:{configuration.TcpPort}.");
            return bootstrap.BindAsync(configuration.TcpPort)
                .ContinueWith(i => bootstrapChannel = i.Result);
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
