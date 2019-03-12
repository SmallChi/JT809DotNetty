using JT809.DotNetty.Core.Clients;
using JT809.DotNetty.Core.Configurations;
using JT809.DotNetty.Core.Interfaces;
using JT809.DotNetty.Core.Metadata;
using JT809.DotNetty.Core.Session;
using JT809.Protocol;
using JT809.Protocol.Enums;
using JT809.Protocol.Extensions;
using JT809.Protocol.MessageBody;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JT809.DotNetty.Core.Internal
{
    /// <summary>
    /// 上级平台
    /// 从链路登录服务实现
    /// </summary>
    class JT809SubordinateLoginImplService : IJT809SubordinateLoginService
    {

        private readonly JT809SubordinateClient subordinateLinkClient;
        private readonly JT809Configuration configuration;

        public JT809SubordinateLoginImplService(
            IOptions<JT809Configuration> jT809ConfigurationAccessor,
            JT809SubordinateClient subordinateLinkClient
            )
        {
            this.subordinateLinkClient = subordinateLinkClient;
            configuration = jT809ConfigurationAccessor.Value;
        }

        public void Login(string ip, int port)
        {
            if (configuration.SubordinateClientEnable)
            {
                subordinateLinkClient.ConnectAsync(ip, port);
            }
        }
    }
}
