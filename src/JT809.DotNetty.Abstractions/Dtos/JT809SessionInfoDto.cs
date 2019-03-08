using System;

namespace JT809.DotNetty.Abstractions.Dtos
{
    public class JT809SessionInfoDto
    {
        /// <summary>
        /// 最后上线时间
        /// </summary>
        public string LastActiveTime { get; set; }
        /// <summary>
        /// 上线时间
        /// </summary>
        public string StartTime { get; set; }
        /// <summary>
        /// 通道是否打开
        /// </summary>
        public bool Open { get; set; }
        /// <summary>
        /// 通道是否活跃
        /// </summary>
        public bool Active { get; set; }
        /// <summary>
        /// 远程IP地址
        /// </summary>
        public string RemoteIP { get; set; }
    }
}
