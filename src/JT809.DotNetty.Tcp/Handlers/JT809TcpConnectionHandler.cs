using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using JT809.DotNetty.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace JT809.DotNetty.Tcp.Handlers
{
    /// <summary>
    /// JT809服务通道处理程序
    /// </summary>
    internal class JT809TcpConnectionHandler : ChannelHandlerAdapter
    {
        private readonly ILogger<JT809TcpConnectionHandler> logger;

        private readonly JT809TcpSessionManager jT809SessionManager;

        public JT809TcpConnectionHandler(
            JT809TcpSessionManager jT809SessionManager,
            ILoggerFactory loggerFactory)
        {
            this.jT809SessionManager = jT809SessionManager;
            logger = loggerFactory.CreateLogger<JT809TcpConnectionHandler>();
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
        /// 设备主动断开
        /// </summary>
        /// <param name="context"></param>
        public override void ChannelInactive(IChannelHandlerContext context)
        {
            string channelId = context.Channel.Id.AsShortText();
            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug($">>>{ channelId } The client disconnects from the server.");
            jT809SessionManager.RemoveSessionByChannel(context.Channel);
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
            jT809SessionManager.RemoveSessionByChannel(context.Channel);
            return base.CloseAsync(context);
        }

        public override void ChannelReadComplete(IChannelHandlerContext context)=> context.Flush();

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
                if(idleStateEvent.State== IdleState.ReaderIdle)
                {
                    string channelId = context.Channel.Id.AsShortText();
                    logger.LogInformation($"{idleStateEvent.State.ToString()}>>>{channelId}");
                    jT809SessionManager.RemoveSessionByChannel(context.Channel);
                    context.CloseAsync();
                }
            }
            base.UserEventTriggered(context, evt);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            string channelId = context.Channel.Id.AsShortText();
            logger.LogError(exception,$"{channelId} {exception.Message}" );
            jT809SessionManager.RemoveSessionByChannel(context.Channel);
            context.CloseAsync();
        }
    }
}

