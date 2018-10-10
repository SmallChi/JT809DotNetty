using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Libuv;
using JT809Netty.Core.Configs;
using JT809Netty.Core.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JT809Netty.Core
{
    /// <summary>
    /// 下级平台从链路
    /// </summary>
    public class JT809DownSlaveLinkNettyService : IHostedService
    {
        IEventLoopGroup bossGroup;

        IEventLoopGroup workerGroup;

        IChannel boundChannel;

        readonly IServiceProvider serviceProvider;

        readonly JT809NettyOptions nettyOptions;

        public JT809DownSlaveLinkNettyService(
            IOptions<JT809NettyOptions> nettyOptionsAccessor,
            IServiceProvider serviceProvider)
        {
            nettyOptions = nettyOptionsAccessor.Value;
            this.serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var dispatcher = new DispatcherEventLoopGroup();
                bossGroup = dispatcher;
                workerGroup = new WorkerEventLoopGroup(dispatcher);
                var bootstrap = new ServerBootstrap();
                bootstrap.Group(bossGroup, workerGroup);
                bootstrap.Channel<TcpServerChannel>();
                bootstrap
                       //.Handler(new LoggingHandler("SRV-LSTN"))
                       .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                       {
                           InitChannel(channel);
                       }))
                       .Option(ChannelOption.SoBacklog, 1048576);
                if (nettyOptions.Host == "")
                {
                    boundChannel = bootstrap.BindAsync(nettyOptions.Port).Result;
                }
                else
                {
                    boundChannel = bootstrap.BindAsync(nettyOptions.Host, nettyOptions.Port).Result;
                }
            }
            catch (Exception ex)
            {

            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                Task.WhenAll(
                  bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                  workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                  boundChannel.CloseAsync());
            }
            catch (Exception ex)
            {

            }
            return Task.CompletedTask;
        }

        private void InitChannel(IChannel channel)
        {
            var scope = serviceProvider.CreateScope();
            //下级平台连续 3min 未收到上级平台发送的从链路保持应答数据包，则认为上级平台的连接中断，将主动断开数据传输从链路。
            channel.Pipeline.AddLast("systemIdleState", new IdleStateHandler(180, 0, 0));
            channel.Pipeline.AddLast("jt809DownSlaveLinkConnection", scope.ServiceProvider.GetRequiredService<JT809DownSlaveLinkConnectionHandler>()); 
            channel.Pipeline.AddLast("jt809Buffer", new DelimiterBasedFrameDecoder(int.MaxValue, Unpooled.CopiedBuffer(new byte[] { JT809.Protocol.JT809Package.BEGINFLAG }), Unpooled.CopiedBuffer(new byte[] { JT809.Protocol.JT809Package.ENDFLAG })));
            channel.Pipeline.AddLast("jt809Decode", scope.ServiceProvider.GetRequiredService<JT809DecodeHandler>());
            //channel.Pipeline.AddLast("jt809Service", scope.ServiceProvider.GetRequiredService<JT808ServiceHandler>());
            scope.Dispose();
        }
    }
}
