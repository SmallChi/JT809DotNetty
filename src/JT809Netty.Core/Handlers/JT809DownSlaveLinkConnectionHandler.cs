using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using JT809Netty.Core.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace JT809Netty.Core.Handlers
{
    /// <summary>
    /// 下级平台从链路
    /// </summary>
    public class JT809DownSlaveLinkConnectionHandler : ChannelHandlerAdapter
    {
        private readonly ILogger<JT809DownSlaveLinkConnectionHandler> logger;

        private readonly SessionManager sessionManager;

        private IOptionsMonitor<JT809NettyOptions> optionsMonitor;

        public JT809DownSlaveLinkConnectionHandler(
            IOptionsMonitor<JT809NettyOptions> optionsMonitor,
            SessionManager sessionManager,
            ILoggerFactory loggerFactory)
        {
            this.optionsMonitor = optionsMonitor;
            this.sessionManager = sessionManager;
            logger = loggerFactory.CreateLogger<JT809DownSlaveLinkConnectionHandler>();
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            base.ChannelActive(context);
        }

        /// <summary>
        /// 主动断开
        /// </summary>
        /// <param name="context"></param>
        public override void ChannelInactive(IChannelHandlerContext context)
        {
            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug(">>>The client disconnects from the server.");
            sessionManager.RemoveSessionByID(context.Channel.Id.AsShortText());
            base.ChannelInactive(context);
        }
        /// <summary>
        /// 服务器主动断开
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task CloseAsync(IChannelHandlerContext context)
        {
            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("<<<The server disconnects from the client.");
            return base.CloseAsync(context);
        }

        /// <summary>
        /// 从链路超时策略
        /// </summary>
        /// <param name="context"></param>
        /// <param name="evt"></param>
        public override void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            IdleStateEvent idleStateEvent = evt as IdleStateEvent;
            if (idleStateEvent != null)
            {
                string channelId = context.Channel.Id.AsShortText();
                logger.LogInformation($"{idleStateEvent.State.ToString()}>>>{channelId}");
                switch (idleStateEvent.State)
                {
                    case IdleState.ReaderIdle:
                        //下级平台连续 3min 未收到上级平台发送的从链路保持应答数据包，则认为上级平台的连接中断，将主动断开数据传输从链路。
                        context.CloseAsync();
                        break;
                    //case IdleState.WriterIdle:

                    //    break;
                    //case IdleState.AllIdle:

                    //    break;
                    default:

                        break;
                }
            }
            base.UserEventTriggered(context, evt);
        }
    }
}
