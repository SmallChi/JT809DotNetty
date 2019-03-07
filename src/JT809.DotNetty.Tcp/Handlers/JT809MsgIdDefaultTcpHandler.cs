using JT809.DotNetty.Core;
using JT809.DotNetty.Core.Handlers;
using JT809.DotNetty.Core.Interfaces;
using JT809.DotNetty.Core.Links;
using JT809.DotNetty.Core.Session;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.DotNetty.Tcp.Handlers
{
    /// <summary>
    /// 默认消息处理业务实现
    /// </summary>
    internal class JT809MsgIdDefaultTcpHandler : JT809MainMsgIdHandlerBase
    {
        public JT809MsgIdDefaultTcpHandler(IJT809VerifyCodeGenerator verifyCodeGenerator, 
            JT809SubordinateClient subordinateLinkClient, JT809MainSessionManager sessionManager)
            : base(verifyCodeGenerator, subordinateLinkClient, sessionManager)
        {
        }
    }
}
