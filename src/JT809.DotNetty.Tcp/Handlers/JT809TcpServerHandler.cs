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

namespace JT809.DotNetty.Tcp.Handlers
{
    /// <summary>
    /// JT809服务端处理程序
    /// </summary>
    internal class JT809TcpServerHandler : SimpleChannelInboundHandler<byte[]>
    {
        private readonly JT809MsgIdTcpHandlerBase handler;
        
        private readonly JT809TcpSessionManager jT809SessionManager;

        private readonly JT809TcpAtomicCounterService jT809AtomicCounterService;

        private readonly ILogger<JT809TcpServerHandler> logger;

        public JT809TcpServerHandler(
            ILoggerFactory loggerFactory,
            JT809MsgIdTcpHandlerBase handler,
            JT809TcpAtomicCounterService jT809AtomicCounterService,
            JT809TcpSessionManager jT809SessionManager
            )
        {
            this.handler = handler;
            this.jT809SessionManager = jT809SessionManager;
            this.jT809AtomicCounterService = jT809AtomicCounterService;
            logger = loggerFactory.CreateLogger<JT809TcpServerHandler>();
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
                jT809SessionManager.TryAdd(new JT809TcpSession(ctx.Channel, jT809Package.Header.MsgGNSSCENTERID));
                Func<JT809Request, JT809Response> handlerFunc;
                if (handler.HandlerDict.TryGetValue(jT809Package.Header.BusinessType, out handlerFunc))
                {
                    JT809Response jT808Response = handlerFunc(new JT809Request(jT809Package, msg));
                    if (jT808Response != null)
                    {
                        var sendData = JT809Serializer.Serialize(jT808Response.Package, jT808Response.MinBufferSize);
                        ctx.WriteAndFlushAsync(Unpooled.WrappedBuffer(sendData));
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
