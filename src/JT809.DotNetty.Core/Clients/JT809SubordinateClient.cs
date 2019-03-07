using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using JT809.DotNetty.Core.Codecs;
using JT809.DotNetty.Core.Handlers;
using JT809.DotNetty.Core.Interfaces;
using JT809.DotNetty.Core.Metadata;
using JT809.Protocol;
using JT809.Protocol.Enums;
using JT809.Protocol.Extensions;
using JT809.Protocol.MessageBody;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace JT809.DotNetty.Core.Clients
{
    /// <summary>
    /// 从链路客户端
    /// 针对企业与企业之间
    /// </summary>
    public sealed class JT809SubordinateClient:IDisposable
    {
        private Bootstrap bootstrap;

        private MultithreadEventLoopGroup group;

        private IChannel channel;

        private readonly ILogger<JT809SubordinateClient> logger;

        private readonly ILoggerFactory loggerFactory;

        private readonly IServiceProvider serviceProvider;

        private readonly IJT809VerifyCodeGenerator verifyCodeGenerator;

        private bool disposed = false;

        public JT809SubordinateClient(
            IServiceProvider provider,
            ILoggerFactory loggerFactory,
            IJT809VerifyCodeGenerator verifyCodeGenerator)
        {
            this.serviceProvider = provider;
            this.loggerFactory = loggerFactory;
            this.verifyCodeGenerator = verifyCodeGenerator;
            this.logger = loggerFactory.CreateLogger<JT809SubordinateClient>();
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
                        pipeline.AddLast("jt809SubClientBuffer", new DelimiterBasedFrameDecoder(int.MaxValue,
                                    Unpooled.CopiedBuffer(new byte[] { JT809Package.BEGINFLAG }),
                                    Unpooled.CopiedBuffer(new byte[] { JT809Package.ENDFLAG })));
                        pipeline.AddLast("jt809SubClientSystemIdleState", new IdleStateHandler(180, 60, 200));
                        pipeline.AddLast("jt809SubClientEncode", scope.ServiceProvider.GetRequiredService<JT809Encoder>());
                        pipeline.AddLast("jt809SubClientDecode", scope.ServiceProvider.GetRequiredService<JT809Decoder>());
                        pipeline.AddLast("jt809SubClientConnection", scope.ServiceProvider.GetRequiredService<JT809SubordinateClientConnectionHandler>());
                        pipeline.AddLast("jt809SubClientServer", scope.ServiceProvider.GetRequiredService<JT809SubordinateClientHandler>());
                    }
                }));
        }

        public async void ConnectAsync(string ip,int port,uint verifyCode,int delay=3000)
        {
            logger.LogInformation($"ip:{ip},port:{port},verifycode:{verifyCode}");
            await Task.Delay(delay);
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
                logger.LogError(ex.InnerException, $"ip:{ip},port:{port},verifycode:{verifyCode}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"ip:{ip},port:{port},verifycode:{verifyCode}");
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

        public bool IsOpen
        {
            get
            {
                if (channel == null) return false;
                return channel.Open && channel.Active;
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
                try
                {
                    //发送从链路注销请求
                    var package = JT809BusinessType.从链路注销请求消息.Create(new JT809_0x9003()
                    {
                        VerifyCode = verifyCodeGenerator.Get()
                    });
                    JT809Response jT809Response = new JT809Response(package, 100);
                    channel.WriteAndFlushAsync(jT809Response);
                    logger.LogInformation($"发送从链路注销请求>>>{JT809Serializer.Serialize(package, 100).ToHexString()}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,"发送从链路注销请求");
                }
                finally
                {
                    //清理托管资源
                    channel.CloseAsync();
                    group.ShutdownGracefullyAsync(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3));
                }
            }
            //让类型知道自己已经被释放
            disposed = true;
        }

        ~JT809SubordinateClient()
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
