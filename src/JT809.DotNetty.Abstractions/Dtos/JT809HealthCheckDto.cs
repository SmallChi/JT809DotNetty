using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.DotNetty.Abstractions.Dtos
{
    /// <summary>
    /// 健康检查
    /// </summary>
    public class JT809HealthCheckDto
    {
        /// <summary>
        /// 主链路会话状态集合
        /// </summary>
        public List<JT809SessionInfoDto> MainSessions { get; set; }
        /// <summary>
        /// 从链路会话状态集合
        /// </summary>
        public List<JT809SessionInfoDto> SubordinateSessions { get; set; }
        /// <summary>
        /// 应用程序使用率
        /// </summary>
        public JT809SystemCollectInfoDto SystemCollect { get; set; }
    }
}
