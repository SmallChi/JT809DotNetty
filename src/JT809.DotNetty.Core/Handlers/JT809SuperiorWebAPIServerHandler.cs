using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using JT809.DotNetty.Core.Metadata;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.DotNetty.Core.Handlers
{
    /// <summary>
    /// 上级平台
    /// webapi处理
    /// </summary>
    internal class JT809SuperiorWebAPIServerHandler : SimpleChannelInboundHandler<IFullHttpRequest>
    {
        private static readonly AsciiString TypeJson = AsciiString.Cached("application/json");
        private static readonly AsciiString ServerName = AsciiString.Cached("JT809SuperiorWebAPINetty");
        private static readonly AsciiString ContentTypeEntity = HttpHeaderNames.ContentType;
        private static readonly AsciiString DateEntity = HttpHeaderNames.Date;
        private static readonly AsciiString ContentLengthEntity = HttpHeaderNames.ContentLength;
        private static readonly AsciiString ServerEntity = HttpHeaderNames.Server;
        private readonly JT809SuperiorWebAPIHandlerBase JT809SuperiorWebAPIHandler;
        private readonly ILogger<JT809SuperiorWebAPIServerHandler> logger;

        public JT809SuperiorWebAPIServerHandler(
            JT809SuperiorWebAPIHandlerBase jT809SuperiorWebAPIHandlerBase,
            ILoggerFactory loggerFactory)
        {
            this.JT809SuperiorWebAPIHandler = jT809SuperiorWebAPIHandlerBase;
            logger = loggerFactory.CreateLogger<JT809SuperiorWebAPIServerHandler>();
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, IFullHttpRequest msg)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug($"Uri:{msg.Uri}");
                logger.LogDebug($"Content:{msg.Content.ToString(Encoding.UTF8)}");
            }
            JT809HttpResponse jT808HttpResponse = null;
            if (JT809SuperiorWebAPIHandler.HandlerDict.TryGetValue(msg.Uri, out var funcHandler))
            {
                jT808HttpResponse = funcHandler(new JT809HttpRequest() { Json = msg.Content.ToString(Encoding.UTF8) });
            }
            else
            {
                jT808HttpResponse = JT809SuperiorWebAPIHandler.NotFoundHttpResponse();
            }
            if (jT808HttpResponse != null)
            {
                WriteResponse(ctx, Unpooled.WrappedBuffer(jT808HttpResponse.Data), TypeJson, jT808HttpResponse.Data.Length);
            }
        }

        private void WriteResponse(IChannelHandlerContext ctx, IByteBuffer buf, ICharSequence contentType, int contentLength)
        {
            // Build the response object.
            var response = new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.OK, buf, false);
            HttpHeaders headers = response.Headers;
            headers.Set(ContentTypeEntity, contentType);
            headers.Set(ServerEntity, ServerName);
            headers.Set(DateEntity, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            headers.Set(ContentLengthEntity, contentLength);
            // Close the non-keep-alive connection after the write operation is done.
            ctx.WriteAndFlushAsync(response);
            ctx.CloseAsync();
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            WriteResponse(context, Unpooled.WrappedBuffer(JT809SuperiorWebAPIHandler.ErrorHttpResponse(exception).Data), TypeJson, JT809SuperiorWebAPIHandler.ErrorHttpResponse(exception).Data.Length);
            logger.LogError(exception, exception.Message);
            context.CloseAsync();
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();
    }
}
