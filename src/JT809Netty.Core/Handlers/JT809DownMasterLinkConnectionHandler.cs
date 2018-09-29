using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using JT809Netty.Core.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace JT809Netty.Core.Handlers
{
    /// <summary>
    /// 下级平台主链路
    /// </summary>
    public class JT809DownMasterLinkConnectionHandler : ChannelHandlerAdapter
    {
        private readonly ILogger<JT809DownMasterLinkConnectionHandler> logger;

        private IOptionsMonitor<JT809NettyOptions> optionsMonitor;

        public JT809DownMasterLinkConnectionHandler(
            IOptionsMonitor<JT809NettyOptions> optionsMonitor,
            SessionManager sessionManager,
            ILoggerFactory loggerFactory)
        {
            this.optionsMonitor = optionsMonitor;
            logger = loggerFactory.CreateLogger<JT809DownMasterLinkConnectionHandler>();
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
        /// 主链路超时策略
        /// 下级平台登录成功后，在与上级平台之间如果有应用业务数据包往来的情况下，不需要发送主链路保持数据包; 
        ///  否则，下级平台应每 1min 发送一个主链路保持清求数据包到上级平台以保持链路连接
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
                    //case IdleState.ReaderIdle:

                    //    break;
                    case IdleState.WriterIdle:
#warning 发送心跳保持
                        break;
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
