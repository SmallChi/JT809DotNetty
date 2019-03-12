using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JT809.DotNetty.Core.Interfaces
{
    /// <summary>
    /// 上级平台
    /// 从链路登录服务
    /// </summary>
    public interface IJT809SubordinateLoginService
    {
        void Login(string ip, int port);
    }
}
