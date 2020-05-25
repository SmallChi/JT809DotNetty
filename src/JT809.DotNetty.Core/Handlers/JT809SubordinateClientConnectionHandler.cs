using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using JT809.DotNetty.Core.Clients;
using JT809.DotNetty.Core.Interfaces;
using JT809.DotNetty.Core.Metadata;
using JT809.Protocol;
using JT809.Protocol.Enums;
using JT809.Protocol.Extensions;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Threading.Tasks;

namespace JT809.DotNetty.Core.Handlers
{
    /// <summary>
    /// JT809 从链路客户端连接处理器
    /// </summary>
    internal class JT809SubordinateClientConnectionHandler : ChannelHandlerAdapter
    {

        private readonly ILogger<JT809SubordinateClientConnectionHandler> logger;
        private readonly JT809SubordinateClient subordinateClient;
        private readonly IJT809SubordinateLinkNotifyService JT809SubordinateLinkNotifyService;
        private readonly JT809Serializer JT809Serializer;

        public JT809SubordinateClientConnectionHandler(
            IJT809Config jT809Config,
            IJT809SubordinateLinkNotifyService jT809SubordinateLinkNotifyService,
            JT809SubordinateClient jT809SubordinateClient,
            ILoggerFactory loggerFactory)
        {
            JT809Serializer = jT809Config.GetSerializer();
            logger = loggerFactory.CreateLogger<JT809SubordinateClientConnectionHandler>();
            JT809SubordinateLinkNotifyService = jT809SubordinateLinkNotifyService;
            subordinateClient = jT809SubordinateClient;
        }

        /// <summary>
        /// 通道激活
        /// </summary>
        /// <param name="context"></param>
        public override void ChannelActive(IChannelHandlerContext context)
        {
            string channelId = context.Channel.Id.AsShortText();
            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug($"<<<{ channelId } Successful client connection to server.");
            base.ChannelActive(context);
        }

        /// <summary>
        /// 客户端主动断开
        /// </summary>
        /// <param name="context"></param>
        public override void ChannelInactive(IChannelHandlerContext context)
        {
            Policy.HandleResult(context.Channel.Open)
                     .WaitAndRetryForeverAsync(retryAttempt =>
                     {
                         if(retryAttempt < 3)
                         {

                             return TimeSpan.FromSeconds(10);
                         }
                         else
                         {
                             JT809SubordinateLinkNotifyService.Notify(JT809_0x9007_ReasonCode.上级平台客户端与下级平台服务端断开);
                             //超过重试3次，之后重试都是接近1个小时重试一次
                             return TimeSpan.FromSeconds(3600);
                         }
                     },
                    (exception, timespan, ctx) =>
                    {
                        logger.LogError($"服务端断开{context.Channel.RemoteAddress}，重试结果{exception.Result}，重试次数{timespan}，下次重试间隔(s){ctx.TotalSeconds}");
                    })
                    .ExecuteAsync(async () => await subordinateClient.ReConnectAsync(context.Channel.RemoteAddress));
            string channelId = context.Channel.Id.AsShortText();
            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug($">>>{ channelId } The client disconnects from the server.");
            base.ChannelInactive(context);
        }

        /// <summary>
        /// 服务器主动断开
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task CloseAsync(IChannelHandlerContext context)
        {
            string channelId = context.Channel.Id.AsShortText();
            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug($"<<<{ channelId } The server disconnects from the client.");
            return base.CloseAsync(context);
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        /// <summary>
        /// 超时策略
        /// </summary>
        /// <param name="context"></param>
        /// <param name="evt"></param>
        public override void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            IdleStateEvent idleStateEvent = evt as IdleStateEvent;
            if (idleStateEvent != null)
            {
                if (idleStateEvent.State == IdleState.WriterIdle)
                {
                    string channelId = context.Channel.Id.AsShortText();
                    //发送从链路保持请求数据包
                    var package = JT809BusinessType.从链路连接保持请求消息.Create();
                    JT809Response jT809Response = new JT809Response(package, 100);
                    if (logger.IsEnabled(LogLevel.Information))
                        logger.LogInformation($"{idleStateEvent.State.ToString()}>>>Heartbeat-{channelId}-{JT809Serializer.Serialize(package, 100).ToHexString()}");
                    context.WriteAndFlushAsync(jT809Response);
                    //context.WriteAndFlushAsync(Unpooled.WrappedBuffer(JT809Serializer.Serialize(package,100)));
                }
            }
            base.UserEventTriggered(context, evt);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            string channelId = context.Channel.Id.AsShortText();
            logger.LogError(exception, $"{channelId} {exception.Message}");
            context.CloseAsync();
        }
    }
}
