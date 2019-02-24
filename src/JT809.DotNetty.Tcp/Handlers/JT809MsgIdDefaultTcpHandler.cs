using JT809.DotNetty.Core;
using JT809.DotNetty.Core.Handlers;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.DotNetty.Tcp.Handlers
{
    /// <summary>
    /// 默认消息处理业务实现
    /// </summary>
    internal class JT809MsgIdDefaultTcpHandler : JT809MsgIdTcpHandlerBase
    {
        public JT809MsgIdDefaultTcpHandler(JT809TcpSessionManager sessionManager) : base(sessionManager)
        {
        }
    }
}
