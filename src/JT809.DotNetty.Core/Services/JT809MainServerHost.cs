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
using JT809.DotNetty.Core.Handlers;

namespace JT809.DotNetty.Core.Services
{
    /// <summary>
    /// JT809 Tcp网关服务
    /// </summary>
    internal class JT809MainServerHost : IHostedService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly JT809Configuration configuration;
        private readonly ILogger<JT809MainServerHost> logger;
        private DispatcherEventLoopGroup bossGroup;
        private WorkerEventLoopGroup workerGroup;
        private IChannel bootstrapChannel;
        private IByteBufferAllocator serverBufferAllocator;
        private ILoggerFactory loggerFactory;

        public JT809MainServerHost(
            IServiceProvider provider,
            ILoggerFactory loggerFactory,
            IOptions<JT809Configuration> jT809ConfigurationAccessor)
        {
            serviceProvider = provider;
            configuration = jT809ConfigurationAccessor.Value;
            logger = loggerFactory.CreateLogger<JT809MainServerHost>();
            this.loggerFactory = loggerFactory;
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
                   channel.Pipeline.AddLast("jt809MainBuffer", new DelimiterBasedFrameDecoder(int.MaxValue,
                                  Unpooled.CopiedBuffer(new byte[] { JT809Package.BEGINFLAG }),
                                  Unpooled.CopiedBuffer(new byte[] { JT809Package.ENDFLAG })));
                   channel.Pipeline.AddLast("jt809MainSystemIdleState", new IdleStateHandler(
                                            configuration.ReaderIdleTimeSeconds,
                                            configuration.WriterIdleTimeSeconds,
                                            configuration.AllIdleTimeSeconds));
                   pipeline.AddLast("jt809MainEncode", new JT809Encoder());
                   pipeline.AddLast("jt809MainDecode", new JT809Decoder());
                   channel.Pipeline.AddLast("jt809MainConnection", new JT809MainServerConnectionHandler(loggerFactory));
                   using (var scope = serviceProvider.CreateScope())
                   {
                       channel.Pipeline.AddLast("jt809MainService", scope.ServiceProvider.GetRequiredService<JT809MainServerHandler>());
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
