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
using JT809.Protocol.Configs;
using JT809.Protocol.Extensions;
using JT809.Protocol.MessageBody;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JT809.DotNetty.Core.Clients
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

        private readonly IServiceProvider serviceProvider;

        private bool disposed = false;

        private readonly IJT809ManualResetEvent manualResetEvent;

        private readonly JT809HeaderOptions JT809HeaderOptions;

        public JT809MainClient(
            JT809HeaderOptions jT809HeaderOptions,
            IJT809ManualResetEvent jT809ManualResetEvent,
            IServiceProvider provider,
            ILoggerFactory loggerFactory)
        {
            JT809HeaderOptions = jT809HeaderOptions;
            this.serviceProvider = provider;
            this.logger = loggerFactory.CreateLogger<JT809MainClient>();
            this.manualResetEvent = jT809ManualResetEvent;
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
                      pipeline.AddLast("jt809MainClientBuffer", new DelimiterBasedFrameDecoder(int.MaxValue,
                           Unpooled.CopiedBuffer(new byte[] { JT809Package.BEGINFLAG }),
                           Unpooled.CopiedBuffer(new byte[] { JT809Package.ENDFLAG })));
                      pipeline.AddLast("jt809MainClientSystemIdleState", new IdleStateHandler(180, 60, 200));
                      pipeline.AddLast("jt809MainClientEncode", scope.ServiceProvider.GetRequiredService<JT809Encoder>());
                      pipeline.AddLast("jt809MainClientDecode", scope.ServiceProvider.GetRequiredService<JT809Decoder>());
                      pipeline.AddLast("jt809MainClientConnection", scope.ServiceProvider.GetRequiredService<JT809MainClientConnectionHandler>());
                      pipeline.AddLast("jt809MainClientServer", scope.ServiceProvider.GetRequiredService<JT809MainClientHandler>());
                  }
              }));
        }

        private JT809_0x1001 _jT809_0x1001;
        private IPEndPoint iPEndPoint;
        public async Task<bool> Login(
            string ip, 
            int port,
            JT809_0x1001 jT809_0x1001)
        {
            if (disposed) return await Task.FromResult(false);
            logger.LogInformation($"ip:{ip},port:{port}");
            this._jT809_0x1001 = jT809_0x1001;
            this.iPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            bool successed = false;
            try
            {
                //IPAddress[] hostinfo = Dns.GetHostAddresses(host);
                //IPAddress address = hostinfo[0];
                if (channel == null)
                {
                    channel = await bootstrap.ConnectAsync(iPEndPoint);
                    successed = channel.Open && channel.Active;
                    if (channel.Open && channel.Active)
                    {
                        //JT809.Protocol.MessageBody.JT809_0x1001 jT809_0X1001 = jT809_0X1001 new Protocol.MessageBody.JT809_0x1001();
                        //jT809_0X1001.DownLinkIP = downLinkIP;
                        //jT809_0X1001.DownLinkPort = downLinkPort;
                        //jT809_0X1001.UserId = userId;
                        //jT809_0X1001.Password = password;
                        var package = JT809.Protocol.Enums.JT809BusinessType.主链路登录请求消息.Create(_jT809_0x1001);
                        package.Header.MsgGNSSCENTERID = JT809HeaderOptions.MsgGNSSCENTERID;
                        package.Header.Version = JT809HeaderOptions.Version;
                        package.Header.EncryptKey = JT809HeaderOptions.EncryptKey;
                        package.Header.EncryptFlag = JT809HeaderOptions.EncryptFlag;
                        await channel.WriteAndFlushAsync(new JT809Response(package, 1024));
                        logger.LogInformation("等待登录应答结果...");
                        manualResetEvent.Pause();
                    }
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
            return await Task.FromResult(successed);
        }
        
        public async void SendAsync(JT809Response jT809Response)
        {
            if (disposed) return;
            if (channel == null) throw new NullReferenceException("Channel Not Open");
            if (jT809Response == null) throw new ArgumentNullException("Data is null");
            if (channel.Open && channel.Active)
            {
                manualResetEvent.Pause();
                await channel.WriteAndFlushAsync(jT809Response);
            }
            else
            {
                manualResetEvent.Reset();
                _ = Policy.HandleResult(channel.Open && channel.Active)
                        .WaitAndRetryForeverAsync(retryAttempt =>
                        {
                            return TimeSpan.FromSeconds(10);
                        }, (exception, timespan, ctx) =>
                         {
                             logger.LogError($"服务端断开{channel.RemoteAddress}，重试结果{exception.Result}，重试次数{timespan}，下次重试间隔(s){ctx.TotalSeconds}");
                         })
                        .ExecuteAsync(async () =>
                        {
                            channel = await bootstrap.ConnectAsync(iPEndPoint);
                            var package = JT809.Protocol.Enums.JT809BusinessType.主链路登录请求消息.Create(_jT809_0x1001);
                            await channel.WriteAndFlushAsync(new JT809Response(package, 100));
                            logger.LogInformation("尝试重连,等待登录应答结果...");
                            manualResetEvent.Pause();
                            return await Task.FromResult(channel.Open && channel.Active);
                        });
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
