using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JT808.Protocol;
using JT808.Protocol.Extensions;
using DotNetty.Common.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using JT808.Protocol.Exceptions;
using System.Threading;

namespace GPS.JT808NettyServer.Handlers
{
    public class JT808ServiceHandler : ChannelHandlerAdapter
    {
        private readonly ILogger<JT808ServiceHandler> logger;

        private readonly JT808MsgIdHandler jT808MsgIdHandler;

        public JT808ServiceHandler(
            JT808MsgIdHandler jT808MsgIdHandler,
            ILoggerFactory loggerFactory)
        {
            this.jT808MsgIdHandler = jT808MsgIdHandler;
            logger = loggerFactory.CreateLogger<JT808ServiceHandler>();
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var jT808RequestInfo = (JT808RequestInfo)message;
            string receive = string.Empty;
            try
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    receive = jT808RequestInfo.OriginalBuffer.ToHexString();
                }
                Func<JT808RequestInfo, IChannelHandlerContext,IJT808Package> handlerFunc;
                if (jT808RequestInfo.JT808Package != null)
                {
                    if (jT808MsgIdHandler.HandlerDict.TryGetValue(jT808RequestInfo.JT808Package.Header.MsgId, out handlerFunc))
                    {
                        IJT808Package jT808PackageImpl = handlerFunc(jT808RequestInfo, context);
                        if (jT808PackageImpl != null)
                        {
                            if (logger.IsEnabled(LogLevel.Debug))
                            {
                                logger.LogDebug("send>>>" + jT808PackageImpl.JT808Package.Header.MsgId.ToString() + "-" + JT808Serializer.Serialize(jT808PackageImpl.JT808Package).ToHexString());
                                //logger.LogDebug("send>>>" + jT808PackageImpl.JT808Package.Header.MsgId.ToString() + "-" + JsonConvert.SerializeObject(jT808PackageImpl.JT808Package));
                            }
                            // 需要注意：
                            // 1.下发应答必须要在类中重写 ChannelReadComplete 不然客户端接收不到消息
                            // context.WriteAsync(Unpooled.WrappedBuffer(JT808Serializer.Serialize(jT808PackageImpl.JT808Package)));
                            // 2.直接发送
                            context.WriteAndFlushAsync(Unpooled.WrappedBuffer(JT808Serializer.Serialize(jT808PackageImpl.JT808Package)));
                        }
                    }
                }
            }
            catch (JT808Exception ex)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError(ex, "JT808Exception receive<<<" + receive);
            }
            catch (Exception ex)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError(ex, "Exception receive<<<" + receive);
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();
    }
}
