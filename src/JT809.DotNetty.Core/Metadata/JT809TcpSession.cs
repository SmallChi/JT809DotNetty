using DotNetty.Transport.Channels;
using System;
using JT809.Protocol.Enums;

namespace JT809.DotNetty.Core.Metadata
{
    public class JT809TcpSession
    {
        public JT809TcpSession(IChannel channel, uint msgGNSSCENTERID)
        {
            MsgGNSSCENTERID = msgGNSSCENTERID;
            Channel = channel;
            StartTime = DateTime.Now;
            LastActiveTime = DateTime.Now;
        }

        /// <summary>
        /// 下级平台接入码，上级平台给下级平台分配唯一标识码。
        /// </summary>
        public uint MsgGNSSCENTERID { get; set; }

        public IChannel Channel { get;}

        public DateTime LastActiveTime { get; set; }

        public DateTime StartTime { get; }
    }
}
