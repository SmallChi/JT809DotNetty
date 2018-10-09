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
using Newtonsoft.Json;

namespace JT809Netty.Core.ServiceHandlers
{
    public class JT809BusinessTypeHandler
    {
        public Dictionary<JT809BusinessType, Func<IChannelHandlerContext, JT809Package>> RequestHandlerDict { get; }

        public Dictionary<JT809BusinessType, Action<JT809Package, IChannelHandlerContext>> ResponseHandlerDict { get; }

        private readonly ILogger<JT809BusinessTypeHandler> logger;

        /// <summary>
        /// 初始化业务处理
        /// </summary>
        public JT809BusinessTypeHandler(
                ILoggerFactory loggerFactory
            )
        {
            logger = loggerFactory.CreateLogger<JT809BusinessTypeHandler>();
            //RequestHandlerDict = new Dictionary<JT809BusinessType, Func<IChannelHandlerContext, JT809Package>>
            //{
            //    {JT809.Protocol.JT809Enums.JT809BusinessType.UP_CONNECT_REQ, Msg0x1001},
            //    {JT809.Protocol.JT809Enums.JT809BusinessType.UP_DISCONNECT_REQ, Msg0x1003},
            //    {JT809.Protocol.JT809Enums.JT809BusinessType.UP_LINKTEST_REQ, Msg0x1005},
            //    //{JT809.Protocol.JT809Enums.JT809BusinessType.UP_DISCONNECT_INFORM, Msg0x1007},
            //    //{JT809.Protocol.JT809Enums.JT809BusinessType.UP_CLOSELINK_INFORM, Msg0x1008},
            //};
            ResponseHandlerDict = new Dictionary<JT809BusinessType, Action<JT809Package, IChannelHandlerContext>>
            {
                {JT809.Protocol.JT809Enums.JT809BusinessType.UP_CONNECT_RSP, Msg0x1002},
                {JT809.Protocol.JT809Enums.JT809BusinessType.UP_DISCONNECT_RSP, Msg0x1004},
                {JT809.Protocol.JT809Enums.JT809BusinessType.UP_LINKTEST_RSP, Msg0x1006},
            };
        }

        public JT809Package Msg0x1001(IChannelHandlerContext channelHandlerContext)
        {
            JT809Package loginPackage = JT809BusinessType.UP_CONNECT_REQ.Create(new JT809_0x1001
            {
                  UserId=1234,
                  DownLinkIP="127.0.0.1",
                  DownLinkPort=8091,
                  Password="20181009"
            });
            try
            {
                byte[] sendLoginData = JT809Serializer.Serialize(loginPackage, 1000);
                channelHandlerContext.WriteAndFlushAsync(Unpooled.WrappedBuffer(sendLoginData));
                return loginPackage;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public void Msg0x1002(JT809Package jT809Package, IChannelHandlerContext channelHandlerContext)
        {
            logger.LogDebug(JsonConvert.SerializeObject(jT809Package));
        }

        public JT809Package Msg0x1003(IChannelHandlerContext channelHandlerContext)
        {
            return null;
        }

        public void Msg0x1004(JT809Package jT809Package, IChannelHandlerContext channelHandlerContext)
        {

        }

        public JT809Package Msg0x1005(IChannelHandlerContext channelHandlerContext)
        {
            JT809Package heartbeatPackage = JT809BusinessType.UP_LINKTEST_REQ.Create(new JT809_0x1005());
            byte[] sendHeartbeatData = JT809Serializer.Serialize(heartbeatPackage, 100);
            channelHandlerContext.WriteAndFlushAsync(Unpooled.WrappedBuffer(sendHeartbeatData));
            return heartbeatPackage;
        }

        public void Msg0x1006(JT809Package jT809Package, IChannelHandlerContext channelHandlerContext)
        {

        }

        public JT809Package Msg0x1007(IChannelHandlerContext channelHandlerContext)
        {
            return null;
        }

        public void Msg0x1008(JT809Package jT809Package, IChannelHandlerContext channelHandlerContext)
        {

        }
    }
}
