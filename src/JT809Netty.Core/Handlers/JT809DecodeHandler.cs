using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using JT809.Protocol;
using JT809.Protocol.JT809Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace JT809Netty.Core.Handlers
{
    /// <summary>
    /// JT809解码
    /// </summary>
    public class JT809DecodeHandler : ByteToMessageDecoder
    {
        private readonly ILogger<JT809DecodeHandler> logger;

        public JT809DecodeHandler(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<JT809DecodeHandler>();
        }

        private static readonly AtomicCounter MsgSuccessCounter = new AtomicCounter();

        private static readonly AtomicCounter MsgFailCounter = new AtomicCounter();

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            string msg = string.Empty;
            byte[] buffer = null;
            try
            {
                buffer = new byte[input.Capacity + 2];
                input.ReadBytes(buffer,1, input.Capacity);
                buffer[0] = JT809Package.BEGINFLAG;
                buffer[input.Capacity + 1] = JT809Package.ENDFLAG;
                output.Add(JT809Serializer.Deserialize(buffer));
                MsgSuccessCounter.Increment();
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    msg = ByteBufferUtil.HexDump(buffer);
                    logger.LogDebug("accept package <<<" + msg);
                    logger.LogDebug("accept package success count<<<" + MsgSuccessCounter.Count.ToString());
                }
            }
            catch (JT809Exception ex)
            {
                MsgFailCounter.Increment();
                logger.LogError("accept package fail count<<<" + MsgFailCounter.Count.ToString());
                logger.LogError(ex, $"{ex.ErrorCode.ToString()}accept msg<<<{msg}");
                return;
            }
            catch (Exception ex)
            {
                MsgFailCounter.Increment();
                logger.LogError("accept package fail count<<<" + MsgFailCounter.Count.ToString());
                logger.LogError(ex, "accept msg<<<" + msg);
                return;
            }
        }
    }
}
