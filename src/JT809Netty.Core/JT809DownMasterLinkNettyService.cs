using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using JT809Netty.Core.Configs;
using JT809Netty.Core.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JT809Netty.Core
{
    /// <summary>
    /// 下级平台主链路
    /// </summary
    public class JT809DownMasterLinkNettyService : IHostedService
    {
        IEventLoopGroup workerGroup;

        Bootstrap bootstrap;

        readonly IServiceProvider serviceProvider;

        readonly IOptionsMonitor<JT809NettyOptions> nettyOptions;

        public JT809DownMasterLinkNettyService(
            IOptionsMonitor<JT809NettyOptions> nettyOptionsAccessor,
            IServiceProvider serviceProvider)
        {
            nettyOptions = nettyOptionsAccessor;
            this.serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            nettyOptions.OnChange(options =>
            {
                try
                {
                    bootstrap.ConnectAsync(options.Host, options.Port);
                }
                catch (Exception ex)
                {

                }
            });
            try
            {
                workerGroup = new MultithreadEventLoopGroup();
                bootstrap = new Bootstrap();
                bootstrap.Group(workerGroup)
                         .Channel<TcpServerChannel>()
                         .Handler(new ActionChannelInitializer<IChannel>(channel =>
                           {
                               InitChannel(channel);
                         }))
                       .Option(ChannelOption.SoBacklog, 1048576);
                bootstrap.ConnectAsync(nettyOptions.CurrentValue.Host, nettyOptions.CurrentValue.Port);
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
                Task.WhenAll(workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
            }
            catch (Exception ex)
            {

            }
            return Task.CompletedTask;
        }

        private void InitChannel(IChannel channel)
        {
            var scope = serviceProvider.CreateScope();
            //下级平台应每 1min 发送一个主链路保持清求数据包到上级平台以保持链路连接
            channel.Pipeline.AddLast("systemIdleState", new WriteTimeoutHandler(60));
            channel.Pipeline.AddLast("jt809DownMasterLinkConnection", scope.ServiceProvider.GetRequiredService<JT809DownMasterLinkConnectionHandler>()); 
            channel.Pipeline.AddLast("jt809Buffer", new DelimiterBasedFrameDecoder(int.MaxValue, Unpooled.CopiedBuffer(new byte[] { JT809.Protocol.JT809Package.BEGINFLAG }), Unpooled.CopiedBuffer(new byte[] { JT809.Protocol.JT809Package.ENDFLAG })));
            channel.Pipeline.AddLast("jt809Decode", scope.ServiceProvider.GetRequiredService<JT809DecodeHandler>());
            //channel.Pipeline.AddLast("jt809Service", scope.ServiceProvider.GetRequiredService<JT808ServiceHandler>());
            scope.Dispose();
        }
    }
}
