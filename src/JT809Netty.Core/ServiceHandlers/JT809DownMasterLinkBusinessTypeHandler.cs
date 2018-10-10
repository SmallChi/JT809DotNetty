using System;
using System.Collections.Generic;
using System.Text;
using DotNetty.Transport.Channels;
using JT809.Protocol;
using JT809.Protocol.JT809Enums;
using JT809.Protocol.JT809MessageBody;
using JT809.Protocol.JT809Extensions;
using DotNetty.Buffers;
using Microsoft.Extensions.Logging;

namespace JT809Netty.Core.ServiceHandlers
{
    public  class JT809DownMasterLinkBusinessTypeHandler
    {
        public ConnectionStatus Status { get; private set; } = ConnectionStatus.蓄势待发;

        public Dictionary<JT809BusinessType, Func<IChannelHandlerContext, JT809Package>> RequestHandlerDict { get; }

        public Dictionary<JT809BusinessType, Action<JT809Package, IChannelHandlerContext>> ResponseHandlerDict { get; }

        private readonly ILogger<JT809DownMasterLinkBusinessTypeHandler> logger;

        public JT809DownMasterLinkBusinessTypeHandler(
                 ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<JT809DownMasterLinkBusinessTypeHandler>();
            RequestHandlerDict = new Dictionary<JT809BusinessType, Func<IChannelHandlerContext, JT809Package>>
            {
                {JT809.Protocol.JT809Enums.JT809BusinessType.主链路登录请求消息, Msg0x1001},
                {JT809.Protocol.JT809Enums.JT809BusinessType.主链路注销请求消息, Msg0x1003},
                {JT809.Protocol.JT809Enums.JT809BusinessType.主链路连接保持请求消息, Msg0x1005},
                //{JT809.Protocol.JT809Enums.JT809BusinessType.UP_DISCONNECT_INFORM, Msg0x1007},
                //{JT809.Protocol.JT809Enums.JT809BusinessType.UP_CLOSELINK_INFORM, Msg0x1008},
            };
            ResponseHandlerDict = new Dictionary<JT809BusinessType, Action<JT809Package, IChannelHandlerContext>>
            {
                {JT809.Protocol.JT809Enums.JT809BusinessType.主链路登录应答消息, Msg0x1002},
                {JT809.Protocol.JT809Enums.JT809BusinessType.主链路注销应答消息, Msg0x1004},
                {JT809.Protocol.JT809Enums.JT809BusinessType.主链路连接保持应答消息, Msg0x1006},
            };
        }

        public enum ConnectionStatus
        {
            蓄势待发=0,
            下级平台主链路已发送注销请求=1,
            上级级平台主链路已发送注销应答 = 2,
        }

        public JT809Package Msg0x1001(IChannelHandlerContext channelHandlerContext)
        {
            JT809Package loginPackage = JT809BusinessType.主链路登录请求消息.Create(new JT809_0x1001
            {
                UserId = 1234,
                Password = "20181009",
                DownLinkIP = "127.0.0.1",
                DownLinkPort = 8091
            });
            byte[] sendLoginData = JT809Serializer.Serialize(loginPackage, 256);
            channelHandlerContext.WriteAndFlushAsync(Unpooled.WrappedBuffer(sendLoginData));
            return loginPackage;
        }

        public JT809Package Msg0x1001(IChannel channel)
        {
            JT809Package loginPackage = JT809BusinessType.主链路登录请求消息.Create(new JT809_0x1001
            {
                UserId = 1234,
                Password = "20181009",
                DownLinkIP = "127.0.0.1",
                DownLinkPort = 8091
            });
            byte[] sendLoginData = JT809Serializer.Serialize(loginPackage, 256);
            channel.WriteAndFlushAsync(Unpooled.WrappedBuffer(sendLoginData));
            return loginPackage;
        }

        public JT809Package Msg0x1003(IChannelHandlerContext channelHandlerContext)
        {
            JT809Package loginPackage = JT809BusinessType.主链路登录请求消息.Create(new JT809_0x1001
            {
                UserId = 1234,
                Password = "20181009",
                DownLinkIP = "127.0.0.1",
                DownLinkPort = 8091
            });
            byte[] sendLoginData = JT809Serializer.Serialize(loginPackage, 256);
            channelHandlerContext.WriteAndFlushAsync(Unpooled.WrappedBuffer(sendLoginData));
            return loginPackage;
        }

        public JT809Package Msg0x1003(IChannel channel)
        {
            JT809Package logoutPackage = JT809BusinessType.主链路注销请求消息.Create(new JT809_0x1003
            {
                UserId = 1234,
                Password = "20181009",
            });
            byte[] sendLoginData = JT809Serializer.Serialize(logoutPackage, 128);
            channel.WriteAndFlushAsync(Unpooled.WrappedBuffer(sendLoginData));
            Status = ConnectionStatus.下级平台主链路已发送注销请求;
            return logoutPackage;
        }

        public JT809Package Msg0x1005(IChannelHandlerContext channelHandlerContext)
        {
            JT809Package heartbeatPackage = JT809BusinessType.主链路连接保持请求消息.Create();
            byte[] sendHeartbeatData = JT809Serializer.Serialize(heartbeatPackage, 100);
            channelHandlerContext.WriteAndFlushAsync(Unpooled.WrappedBuffer(sendHeartbeatData));
            return heartbeatPackage;
        }

        public JT809Package Msg0x1007(IChannelHandlerContext channelHandlerContext)
        {
            return null;
        }

        public void Msg0x1002(JT809Package jT809Package, IChannelHandlerContext channelHandlerContext)
        {

        }

        public void Msg0x1004(JT809Package jT809Package, IChannelHandlerContext channelHandlerContext)
        {
            Status = ConnectionStatus.上级级平台主链路已发送注销应答;
        }

        public void Msg0x1006(JT809Package jT809Package, IChannelHandlerContext channelHandlerContext)
        {

        }

        public void Msg0x1008(JT809Package jT809Package, IChannelHandlerContext channelHandlerContext)
        {

        }
    }
}
