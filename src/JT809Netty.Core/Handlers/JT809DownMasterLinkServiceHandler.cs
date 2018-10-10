using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DotNetty.Common.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading;
using JT809Netty.Core.ServiceHandlers;
using JT809.Protocol;
using JT809.Protocol.JT809Exceptions;

namespace JT809Netty.Core.Handlers
{
    /// <summary>
    /// 下级平台主链路
    /// </summary>
    public class JT809DownMasterLinkServiceHandler : ChannelHandlerAdapter
    {
        private readonly ILogger<JT809DownMasterLinkServiceHandler> logger;

        private readonly JT809DownMasterLinkBusinessTypeHandler jT809DownMasterLinkBusinessTypeHandler;

        public JT809DownMasterLinkServiceHandler(
                 JT809DownMasterLinkBusinessTypeHandler jT809DownMasterLinkBusinessTypeHandler,
                 ILoggerFactory loggerFactory)
        {
            this.jT809DownMasterLinkBusinessTypeHandler = jT809DownMasterLinkBusinessTypeHandler;
            logger = loggerFactory.CreateLogger<JT809DownMasterLinkServiceHandler>();
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var jT809Package = (JT809Package)message;
            string receive = string.Empty;
            try
            {
                if (logger.IsEnabled(LogLevel.Debug))
                    logger.LogDebug(JsonConvert.SerializeObject(jT809Package));
                if (jT809DownMasterLinkBusinessTypeHandler.ResponseHandlerDict.TryGetValue(jT809Package.Header.BusinessType,out var action))
                {
                    action(jT809Package, context);
                }
            }
            catch (JT809Exception ex)
            {
                if (logger.IsEnabled(LogLevel.Error))
                    logger.LogError(ex, "JT809Exception receive<<<" + receive);
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
