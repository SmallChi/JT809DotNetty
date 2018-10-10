using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using JT809Netty.Core.Configs;
using JT809Netty.Core.Handlers;
using JT809Netty.Core.ServiceHandlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

        IChannel ClientChannel;

        private readonly JT809DownMasterLinkBusinessTypeHandler jT809DownMasterLinkBusinessTypeHandler;

        private readonly ILogger<JT809DownMasterLinkNettyService> logger;

        public JT809DownMasterLinkNettyService(
            ILoggerFactory loggerFactory,
            JT809DownMasterLinkBusinessTypeHandler jT809DownMasterLinkBusinessTypeHandler,
            IOptionsMonitor<JT809NettyOptions> nettyOptionsAccessor,
            IServiceProvider serviceProvider)
        {
            logger = loggerFactory.CreateLogger<JT809DownMasterLinkNettyService>();
            this.jT809DownMasterLinkBusinessTypeHandler = jT809DownMasterLinkBusinessTypeHandler;
            nettyOptions = nettyOptionsAccessor;
            this.serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () => 
            {
                try
                {
                    workerGroup = new MultithreadEventLoopGroup();
                    bootstrap = new Bootstrap();
                    bootstrap.Group(workerGroup)
                             .Channel<TcpSocketChannel>()
                             .Handler(new ActionChannelInitializer<IChannel>(channel =>
                             {
                                 InitChannel(channel);
                             }))
                             .Option(ChannelOption.SoBacklog, 1048576);
                    ClientChannel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(nettyOptions.CurrentValue.Host), nettyOptions.CurrentValue.Port));

                    jT809DownMasterLinkBusinessTypeHandler.Msg0x1001(ClientChannel);
                }
                catch (Exception ex)
                {

                }
            });
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                jT809DownMasterLinkBusinessTypeHandler.Msg0x1003(ClientChannel);
                // 已发送注销请求,等待30s,待服务器响应
                int sleepTime = 50000;
                logger.LogInformation($">>>The logout request has been sent, waiting for {sleepTime/1000}s for the server to respond...");
                Thread.Sleep(sleepTime);
                logger.LogInformation($"Check Status:<<<{jT809DownMasterLinkBusinessTypeHandler.Status.ToString()}");
                ClientChannel.CloseAsync().ContinueWith((state) => {
                    Task.WhenAll(workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex,"");
            }
            return Task.CompletedTask;
        }

        private void InitChannel(IChannel channel)
        {
            var scope = serviceProvider.CreateScope();
            try
            {
                //下级平台应每 1min 发送一个主链路保持清求数据包到上级平台以保持链路连接
                channel.Pipeline.AddLast("systemIdleState", new IdleStateHandler(0, 60, 0));
                channel.Pipeline.AddLast("jt809DownMasterLinkConnection", scope.ServiceProvider.GetRequiredService<JT809DownMasterLinkConnectionHandler>());
                channel.Pipeline.AddLast("jt809Buffer", new DelimiterBasedFrameDecoder(int.MaxValue, Unpooled.CopiedBuffer(new byte[] { JT809.Protocol.JT809Package.BEGINFLAG }), Unpooled.CopiedBuffer(new byte[] { JT809.Protocol.JT809Package.ENDFLAG })));
                channel.Pipeline.AddLast("jt809Decode", scope.ServiceProvider.GetRequiredService<JT809DecodeHandler>());
                channel.Pipeline.AddLast("jT809DownMasterLinkServiceHandler", scope.ServiceProvider.GetRequiredService<JT809DownMasterLinkServiceHandler>());
            }
            finally
            {
                scope.Dispose();
            }
        }
    }
}
