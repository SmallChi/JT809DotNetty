using DotNetty.Buffers;
using DotNetty.Codecs;
using System.Collections.Generic;
using DotNetty.Transport.Channels;
using JT809.Protocol;
using JT809.DotNetty.Core.Metadata;

namespace JT809.DotNetty.Core.Codecs
{
    /// <summary>
    /// JT809编码
    /// </summary>
    internal class JT809Encoder : MessageToByteEncoder<JT809Response>
    {
        protected override void Encode(IChannelHandlerContext context, JT809Response message, IByteBuffer output)
        {
            if (message.Package != null) {
                var sendData = JT809Serializer.Serialize(message.Package, message.MinBufferSize);
                output.WriteBytes(sendData);
            }
            else if (message.HexData != null)
            {
                output.WriteBytes(message.HexData);
            }
        }
    }
}
