using System;
using System.Collections.Generic;
using System.Text;

namespace JT809Netty.Core.Configs
{
    public class JT809NettyOptions
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public List<string> IpWhiteList { get; set; } = new List<string>();
        public bool IpWhiteListDisabled { get; set; }
    }
}
