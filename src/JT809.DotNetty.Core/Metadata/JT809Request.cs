using JT809.Protocol;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace JT809.DotNetty.Core.Metadata
{
    public class JT809Request
    {
        public JT809Package Package { get; }

        /// <summary>
        /// 用于消息发送
        /// </summary>
        public byte[] OriginalPackage { get;}

        public JT809Request(JT809Package package, byte[] originalPackage)
        {
            Package = package;
            OriginalPackage = originalPackage;
        }
    }
}