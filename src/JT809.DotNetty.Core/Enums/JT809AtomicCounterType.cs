using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.DotNetty.Core.Enums
{
    public enum JT809AtomicCounterType
    {
        /// <summary>
        /// 上级平台连接下级平台从链路（企业-企业）
        /// </summary>
        ClientSubordinate=1,
        /// <summary>
        /// 上级平台连接下级平台从链路（监管平台-企业）
        /// </summary>
        ServerSubordinate=2,
        /// <summary>
        /// 上级平台接收下级平台主链路（企业-企业）
        /// </summary>
        ServerMain =3,
        /// <summary>
        /// 下级平台连接上级平台主链路（企业-监管平台）
        /// </summary>
        ClientMain=4
    }
}
