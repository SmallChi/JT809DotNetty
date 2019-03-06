using DotNetty.Buffers;
using DotNetty.Codecs;
using System.Collections.Generic;
using DotNetty.Transport.Channels;
using JT809.Protocol;

namespace JT809.DotNetty.Core.Codecs
{
    /// <summary>
    /// JT809解码
    /// </summary>
    public class JT809Decoder : ByteToMessageDecoder
    {
        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            byte[] buffer = new byte[input.Capacity + 2];
            input.ReadBytes(buffer, 1, input.Capacity);
            buffer[0] = JT809Package.BEGINFLAG;
            buffer[input.Capacity + 1] = JT809Package.ENDFLAG;
            output.Add(buffer);
        }
    }
}
