using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using JT809.Protocol;
using System;
using Microsoft.Extensions.Logging;
using JT809.Protocol.Exceptions;
using JT809.DotNetty.Core.Services;
using JT809.DotNetty.Core.Metadata;
using JT809.DotNetty.Core.Enums;
using JT809.DotNetty.Core.Session;

namespace JT809.DotNetty.Core.Handlers
{
    /// <summary>
    /// 上级平台
    /// JT809主链路服务端处理程序
    /// </summary>
    internal class JT809MainServerHandler : SimpleChannelInboundHandler<byte[]>
    {
        private readonly JT809SuperiorMsgIdReceiveHandlerBase handler;
        
        private readonly JT809AtomicCounterService jT809AtomicCounterService;

        private readonly JT809SuperiorMainSessionManager SuperiorMainSessionManager;

        private readonly ILogger<JT809MainServerHandler> logger;
        private readonly JT809Serializer JT809Serializer;

        public JT809MainServerHandler(
            IJT809Config jT809Config,
            ILoggerFactory loggerFactory,
            JT809SuperiorMsgIdReceiveHandlerBase handler,
            JT809SuperiorMainSessionManager superiorMainSessionManager,
            JT809AtomicCounterServiceFactory jT809AtomicCounterServiceFactorty

            )
        {
            JT809Serializer = jT809Config.GetSerializer();
            this.handler = handler;
            this.jT809AtomicCounterService = jT809AtomicCounterServiceFactorty.Create(JT809AtomicCounterType.ServerMain.ToString()); ;
            this.SuperiorMainSessionManager = superiorMainSessionManager;
            logger = loggerFactory.CreateLogger<JT809MainServerHandler>();
        }


        protected override async void ChannelRead0(IChannelHandlerContext ctx, byte[] msg)
        {
            try
            {
                JT809Package jT809Package = JT809Serializer.Deserialize(msg);
                jT809AtomicCounterService.MsgSuccessIncrement();
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("accept package success count<<<" + jT809AtomicCounterService.MsgSuccessCount.ToString());
                }
                SuperiorMainSessionManager.TryAdd(ctx.Channel, jT809Package.Header.MsgGNSSCENTERID);
                Func<JT809Request, JT809Response> handlerFunc;
                if (handler.HandlerDict.TryGetValue(jT809Package.Header.BusinessType, out handlerFunc))
                {
                    JT809Response jT808Response = handlerFunc(new JT809Request(jT809Package, msg));
                    if (jT808Response != null)
                    {
                        var sendData = JT809Serializer.Serialize(jT808Response.Package, jT808Response.MinBufferSize);
                        await ctx.WriteAndFlushAsync(Unpooled.WrappedBuffer(sendData));
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
