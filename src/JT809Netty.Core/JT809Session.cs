using DotNetty.Transport.Channels;
using System;
using JT809.Protocol.JT809Enums;

namespace JT809Netty.Core
{
    public class JT809Session: IAppSession
    {
        public JT809Session(IChannel channel, string vehicleNo,JT809VehicleColorType vehicleColor)
        {
            Channel = channel;
            VehicleNo = vehicleNo;
            VehicleColor = vehicleColor;
            StartTime = DateTime.Now;
            LastActiveTime = DateTime.Now;
            SessionID = Channel.Id.AsShortText();
            Key = $"{VehicleNo}_{VehicleColor.ToString()}";
        }

        /// <summary>
        /// 车牌号
        /// </summary>
        public string VehicleNo { get; set; }
        /// <summary>
        /// 车牌颜色
        /// </summary>
        public JT809VehicleColorType VehicleColor { get; set; }

        public string Key { get; set; }

        public string SessionID { get; }

        public IChannel Channel { get;}

        public DateTime LastActiveTime { get; set; }

        public DateTime StartTime { get; }
    }
}
