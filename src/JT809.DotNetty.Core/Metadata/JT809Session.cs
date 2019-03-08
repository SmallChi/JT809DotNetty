using DotNetty.Transport.Channels;
using System;
using JT809.Protocol.Enums;

namespace JT809.DotNetty.Core.Metadata
{
    public class JT809Session
    {
        public JT809Session(IChannel channel, uint msgGNSSCENTERID)
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

        public IChannel Channel { get; set; }

        public DateTime LastActiveTime { get; set; }

        public DateTime StartTime { get; }
    }
}
