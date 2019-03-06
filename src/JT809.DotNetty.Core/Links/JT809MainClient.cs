using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using JT809.DotNetty.Core.Codecs;
using JT809.DotNetty.Core.Handlers;
using JT809.DotNetty.Core.Metadata;
using JT809.Protocol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace JT809.DotNetty.Core.Links
{
    /// <summary>
    /// 主链路客户端
    /// 针对企业与监管平台之间
    /// </summary>
    public sealed class JT809MainClient : IDisposable
    {
        private Bootstrap bootstrap;

        private MultithreadEventLoopGroup group;

        private IChannel channel;

        private readonly ILogger<JT809MainClient> logger;

        private readonly ILoggerFactory loggerFactory;

        private readonly IServiceProvider serviceProvider;

        private bool disposed = false;

        public JT809MainClient(
            IServiceProvider provider,
            ILoggerFactory loggerFactory)
        {
            this.serviceProvider = provider;
            this.loggerFactory = loggerFactory;
            this.logger = loggerFactory.CreateLogger<JT809MainClient>();
            group = new MultithreadEventLoopGroup();
            bootstrap = new Bootstrap();
            bootstrap.Group(group)
              .Channel<TcpSocketChannel>()
              .Option(ChannelOption.TcpNodelay, true)
              .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
              {
                  IChannelPipeline pipeline = channel.Pipeline;
                      //下级平台1分钟发送心跳
                      //上级平台是3分钟没有发送就断开连接
                      using (var scope = serviceProvider.CreateScope())
                  {
                      pipeline.AddLast("jt809MainLinkTcpBuffer", new DelimiterBasedFrameDecoder(int.MaxValue,
                           Unpooled.CopiedBuffer(new byte[] { JT809Package.BEGINFLAG }),
                           Unpooled.CopiedBuffer(new byte[] { JT809Package.ENDFLAG })));
                      pipeline.AddLast("jt809MainLinkSystemIdleState", new IdleStateHandler(180, 60, 200));
                      pipeline.AddLast("jt809MainLinkTcpEncode", scope.ServiceProvider.GetRequiredService<JT809Encoder>());
                      pipeline.AddLast("jt809MainLinkTcpDecode", scope.ServiceProvider.GetRequiredService<JT809Decoder>());
                      pipeline.AddLast("jt809MainLinkConnection", scope.ServiceProvider.GetRequiredService<JT809MainServerConnectionHandler>());

                  }
              }));
        }

        public async void ConnectAsync(string ip,int port)
        {
            logger.LogInformation($"ip:{ip},port:{port}");
            try
            {
                if (channel == null)
                {
                    channel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(ip), port));
                }
                else
                {
                    await channel.CloseAsync();
                    channel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(ip), port));
                }
            }
            catch (AggregateException ex)
            {
                logger.LogError(ex.InnerException, $"ip:{ip},port:{port}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ip:{ip},port:{port}");
            }
        }

        public async void SendAsync(JT809Response jT809Response)
        {
            if (channel == null) throw new NullReferenceException("Channel Not Open");
            if (jT809Response == null) throw new ArgumentNullException("Data is null");
            if (channel.Open && channel.Active)
            {
                await channel.WriteAndFlushAsync(jT809Response);
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }
            if (disposing)
            {
                //清理托管资源
                channel.CloseAsync();
                group.ShutdownGracefullyAsync(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3));
            }
            //让类型知道自己已经被释放
            disposed = true;
        }

        ~JT809MainClient()
        {
            //必须为false
            //这表明，隐式清理时，只要处理非托管资源就可以了。
            Dispose(false);
        }

        public void Dispose()
        {
            //必须为true
            Dispose(true);
            //通知垃圾回收机制不再调用终结器（析构器）
            GC.SuppressFinalize(this);
        }
    }
}
