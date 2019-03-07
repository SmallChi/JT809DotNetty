using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using JT809.DotNetty.Core.Metadata;
using JT809.Protocol.Enums;
using JT809.Protocol.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace JT809.DotNetty.Core.Handlers
{
    /// <summary>
    /// JT809主链路客户端连接处理器
    /// </summary>
    internal class JT809MainClientConnectionHandler : ChannelHandlerAdapter
    {

        private readonly ILogger<JT809MainClientConnectionHandler> logger;

        public JT809MainClientConnectionHandler(
            ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<JT809MainClientConnectionHandler>();
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
                    logger.LogInformation($"{idleStateEvent.State.ToString()}>>>Heartbeat-{channelId}");
                    //发送主链路保持请求数据包
                    var package = JT809BusinessType.主链路连接保持请求消息.Create();
                    JT809Response jT809Response = new JT809Response(package, 100);
                    context.WriteAndFlushAsync(jT809Response);
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
