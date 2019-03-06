using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using JT809.Protocol;
using System;
using Microsoft.Extensions.Logging;
using JT809.Protocol.Exceptions;
using JT809.DotNetty.Core.Services;
using JT809.DotNetty.Core;
using JT809.DotNetty.Core.Handlers;
using JT809.DotNetty.Core.Metadata;
using JT809.DotNetty.Core.Internal;
using JT809.DotNetty.Core.Enums;

namespace JT809.DotNetty.Core.Handlers
{
    /// <summary>
    /// JT809从链路业务处理程序
    /// </summary>
    internal class JT809SubordinateServerHandler : SimpleChannelInboundHandler<byte[]>
    {
        private readonly JT809SubordinateMsgIdHandlerBase handler;
        
        private readonly JT809AtomicCounterService jT809AtomicCounterService;

        private readonly ILogger<JT809SubordinateServerHandler> logger;

        public JT809SubordinateServerHandler(
            ILoggerFactory loggerFactory,
            JT809SubordinateMsgIdHandlerBase handler,
            JT809AtomicCounterServiceFactory jT809AtomicCounterServiceFactorty
            )
        {
            this.handler = handler;
            this.jT809AtomicCounterService = jT809AtomicCounterServiceFactorty.Create(JT809AtomicCounterType.ClientSubordinate.ToString());
            logger = loggerFactory.CreateLogger<JT809SubordinateServerHandler>();
        }


        protected override void ChannelRead0(IChannelHandlerContext ctx, byte[] msg)
        {
            try
            {
                JT809Package jT809Package = JT809Serializer.Deserialize(msg);
                jT809AtomicCounterService.MsgSuccessIncrement();
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("accept package success count<<<" + jT809AtomicCounterService.MsgSuccessCount.ToString());
                }
                Func<JT809Request, JT809Response> handlerFunc;
                if (handler.HandlerDict.TryGetValue(jT809Package.Header.BusinessType, out handlerFunc))
                {
                    JT809Response jT808Response = handlerFunc(new JT809Request(jT809Package, msg));
                    if (jT808Response != null)
                    {
                        ctx.WriteAndFlushAsync(jT808Response);
                    }
                }
            }
            catch (JT809Exception ex)
            {
                jT809AtomicCounterService.MsgFailIncrement();
                if (logger.IsEnabled(LogLevel.Error))
                {
                    logger.LogError("accept package fail count<<<" + jT809AtomicCounterService.MsgFailCount.ToString());
                    logger.LogError(ex, "accept msg<<<" + ByteBufferUtil.HexDump(msg));
                }
            }
            catch (Exception ex)
            {
                jT809AtomicCounterService.MsgFailIncrement();
                if (logger.IsEnabled(LogLevel.Error))
                {
                    logger.LogError("accept package fail count<<<" + jT809AtomicCounterService.MsgFailCount.ToString());
                    logger.LogError(ex, "accept msg<<<" + ByteBufferUtil.HexDump(msg));
                }
            }
        }
    }
}
