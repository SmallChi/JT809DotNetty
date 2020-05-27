using JT809.DotNetty.Core.Configurations;
using JT809.DotNetty.Core.Interfaces;
using JT809.DotNetty.Core.Metadata;
using JT809.DotNetty.Core.Session;
using JT809.Protocol;
using JT809.Protocol.Configs;
using JT809.Protocol.Enums;
using JT809.Protocol.Extensions;
using JT809.Protocol.MessageBody;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.DotNetty.Core.Internal
{
    class JT809SubordinateLinkNotifyImplService: IJT809SubordinateLinkNotifyService
    {
        private readonly JT809Configuration configuration;
        private readonly JT809SuperiorMainSessionManager jT809SuperiorMainSessionManager;
        private readonly ILogger logger;
        private readonly JT809Serializer JT809Serializer;
        private readonly JT809HeaderOptions JT809HeaderOptions;

        public JT809SubordinateLinkNotifyImplService(
            JT809HeaderOptions jT809HeaderOptions,
            ILoggerFactory loggerFactory,
            IJT809Config jT809Config,
            IOptions<JT809Configuration> jT809ConfigurationAccessor,
            JT809SuperiorMainSessionManager jT809SuperiorMainSessionManager
            )
        {
            JT809Serializer = jT809Config.GetSerializer();
            JT809HeaderOptions = jT809HeaderOptions;
            this.logger = loggerFactory.CreateLogger<JT809SubordinateLinkNotifyImplService>();
            configuration = jT809ConfigurationAccessor.Value;
            this.jT809SuperiorMainSessionManager = jT809SuperiorMainSessionManager;
        }

        public void Notify(JT809_0x9007_ReasonCode reasonCode)
        {
            Notify(reasonCode, JT809HeaderOptions.MsgGNSSCENTERID);
        }

        public void Notify(JT809_0x9007_ReasonCode reasonCode, uint msgGNSSCENTERID)
        {
            if (configuration.SubordinateClientEnable)
            {
                var session = jT809SuperiorMainSessionManager.GetSession(msgGNSSCENTERID);
                if (session != null)
                {
                    //发送从链路注销请求
                    var package = JT809BusinessType.从链路断开通知消息.Create(new JT809_0x9007()
                    {
                        ReasonCode = reasonCode
                    });
                    package.Header.MsgGNSSCENTERID = msgGNSSCENTERID;
                    package.Header.Version = JT809HeaderOptions.Version;
                    package.Header.EncryptKey = JT809HeaderOptions.EncryptKey;
                    package.Header.EncryptFlag = JT809HeaderOptions.EncryptFlag;
                    JT809Response jT809Response = new JT809Response(package, 100);
                    if (logger.IsEnabled(LogLevel.Information))
                        logger.LogInformation($"从链路断开通知消息>>>{JT809Serializer.Serialize(package, 100).ToHexString()}");
                    session.Channel.WriteAndFlushAsync(jT809Response);
                }
            }
        }
    }
}
