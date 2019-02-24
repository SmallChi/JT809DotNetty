using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using JT809.DotNetty.Core.Codecs;
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
    /// 从链路客户端
    /// </summary>
    public sealed class SubordinateLinkClient:IDisposable
    {
        private Bootstrap bootstrap;

        private MultithreadEventLoopGroup group;

        private IChannel channel;

        private readonly ILogger<SubordinateLinkClient> logger;

        private readonly ILoggerFactory loggerFactory;

        private readonly IServiceProvider serviceProvider;

        private bool disposed = false;

        public SubordinateLinkClient(
            IServiceProvider provider,
            ILoggerFactory loggerFactory)
        {
            this.serviceProvider = provider;
            this.loggerFactory = loggerFactory;
            this.logger = loggerFactory.CreateLogger<SubordinateLinkClient>();
        }

        public async void ConnectAsync(string ip,int port,uint verifyCode)
        {
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
                   channel.Pipeline.AddLast("jt809SystemIdleState", new IdleStateHandler(60,180,200));
                   //pipeline.AddLast(new ClientConnectionHandler(bootstrap, channeldic, loggerFactory));
                   channel.Pipeline.AddLast("jt809TcpBuffer", new DelimiterBasedFrameDecoder(int.MaxValue,
                                    Unpooled.CopiedBuffer(new byte[] { JT809Package.BEGINFLAG }),
                                    Unpooled.CopiedBuffer(new byte[] { JT809Package.ENDFLAG })));
                   using (var scope = serviceProvider.CreateScope())
                   {
                       channel.Pipeline.AddLast("jt809TcpDecode", scope.ServiceProvider.GetRequiredService<JT809TcpDecoder>());

                   }
               }));
            channel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(ip), port));
        }

        public async void SendAsync(byte[] data)
        {
            if (channel == null) throw new NullReferenceException("Channel Not Open");
            if (data == null) throw new ArgumentNullException("data is null");
            if (channel.Open && channel.Active)
            {
                await channel.WriteAndFlushAsync(Unpooled.WrappedBuffer(data));
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

        ~SubordinateLinkClient()
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
