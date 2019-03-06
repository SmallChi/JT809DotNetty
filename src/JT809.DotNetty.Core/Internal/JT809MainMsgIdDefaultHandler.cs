using JT809.DotNetty.Core;
using JT809.DotNetty.Core.Handlers;
using JT809.DotNetty.Core.Interfaces;
using JT809.DotNetty.Core.Links;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.DotNetty.Core.Internal
{
    /// <summary>
    /// 默认主链路服务端消息处理业务实现
    /// </summary>
    internal class JT809MainMsgIdDefaultHandler : JT809MainMsgIdHandlerBase
    {
        public JT809MainMsgIdDefaultHandler(IJT809VerifyCodeGenerator verifyCodeGenerator, 
            JT809SubordinateClient subordinateLinkClient, JT809SessionManager sessionManager)
            : base(verifyCodeGenerator, subordinateLinkClient, sessionManager)
        {
        }
    }
}
