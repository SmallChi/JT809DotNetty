using JT809.Protocol.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JT809.DotNetty.Core.Interfaces
{
    /// <summary>
    /// 上级平台
    /// 从链路服务
    /// </summary>
    public interface IJT809SubordinateLinkNotifyService
    {
        void Notify(JT809_0x9007_ReasonCode reasonCode);

        void Notify(JT809_0x9007_ReasonCode reasonCode,uint msgGNSSCENTERID);
    }
}
