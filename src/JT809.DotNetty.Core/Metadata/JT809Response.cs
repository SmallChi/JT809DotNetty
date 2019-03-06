using JT809.Protocol;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace JT809.DotNetty.Core.Metadata
{
    public class JT809Response
    {
        public JT809Package Package { get; set; }
        /// <summary>
        /// 根据实际情况适当调整包的大小
        /// </summary>
        public int MinBufferSize { get; set; } 

        public byte[] HexData { get; set; }

        public JT809Response()
        {

        }

        public JT809Response(JT809Package package, int minBufferSize = 1024)
        {
            Package = package;
            MinBufferSize = minBufferSize;
        }

        public JT809Response(byte[] hexData)
        {
            HexData = hexData;
        }
    }
}