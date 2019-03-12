using JT809.DotNetty.Core;
using JT809.DotNetty.Core.Configurations;
using JT809.DotNetty.Core.Handlers;
using JT809.DotNetty.Core.Interfaces;
using JT809.DotNetty.Core.Clients;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.DotNetty.Core.Internal
{
    /// <summary>
    /// 默认消息处理业务实现
    /// </summary>
    internal class JT809SuperiorMsgIdReceiveDefaultHandler : JT809SuperiorMsgIdReceiveHandlerBase
    {
        public JT809SuperiorMsgIdReceiveDefaultHandler(ILoggerFactory loggerFactory, IJT809SubordinateLoginService jT809SubordinateLoginService, IJT809VerifyCodeGenerator verifyCodeGenerator) : base(loggerFactory, jT809SubordinateLoginService, verifyCodeGenerator)
        {
        }
    }
}
