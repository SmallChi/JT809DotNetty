using System;
using System.Collections.Generic;
using JT809.DotNetty.Core.Configurations;
using JT809.DotNetty.Core.Interfaces;
using JT809.DotNetty.Core.Clients;
using JT809.DotNetty.Core.Metadata;
using JT809.Protocol.Enums;
using JT809.Protocol.Extensions;
using JT809.Protocol.MessageBody;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JT809.DotNetty.Core.Handlers
{
    /// <summary>
    /// 上级平台
    /// 主从链路接收消息处理程序
    /// </summary>
    public abstract class JT809SuperiorMsgIdReceiveHandlerBase
    {
        protected IJT809VerifyCodeGenerator VerifyCodeGenerator { get; }
        protected JT809SubordinateClient SubordinateLinkClient { get; }
        protected JT809Configuration  Configuration { get; }
        protected ILogger Logger { get; }
        /// <summary>
        /// 初始化消息处理业务
        /// </summary>
        protected JT809SuperiorMsgIdReceiveHandlerBase(
            ILoggerFactory loggerFactory,
            IOptions<JT809Configuration> jT809ConfigurationAccessor,
            IJT809VerifyCodeGenerator verifyCodeGenerator,
            JT809SubordinateClient subordinateLinkClient)
        {
            this.Logger = loggerFactory.CreateLogger<JT809SuperiorMsgIdReceiveHandlerBase>();
            this.VerifyCodeGenerator = verifyCodeGenerator;
            this.SubordinateLinkClient = subordinateLinkClient;
            HandlerDict = new Dictionary<JT809BusinessType, Func<JT809Request, JT809Response>>
            {
                {JT809BusinessType.主链路登录请求消息, Msg0x1001},
                {JT809BusinessType.主链路注销请求消息, Msg0x1003},
                {JT809BusinessType.主链路连接保持请求消息, Msg0x1005},
                {JT809BusinessType.主链路动态信息交换消息, Msg0x1200},

                {JT809BusinessType.从链路注销应答消息, Msg0x9004},
            };

            SubHandlerDict = new Dictionary<JT809SubBusinessType, Func<JT809Request, JT809Response>>
            {
                //{JT809SubBusinessType.实时上传车辆定位信息, Msg0x1200_0x1202},
            };
        }

        public Dictionary<JT809BusinessType, Func<JT809Request, JT809Response>> HandlerDict { get; protected set; }

        public Dictionary<JT809SubBusinessType, Func<JT809Request, JT809Response>> SubHandlerDict { get; protected set; }

        /// <summary>
        /// 主链路登录请求消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns>主链路登录应答消息</returns>
        public virtual JT809Response Msg0x1001(JT809Request request)
        {
            var verifyCode = VerifyCodeGenerator.Create();
            Logger.LogInformation($"VerifyCode-{verifyCode}");
            var package = JT809BusinessType.主链路登录应答消息.Create(new JT809_0x1002()
            {
                Result = JT809_0x1002_Result.成功,
                VerifyCode = verifyCode
            });
            if (Configuration.SubordinateClientEnable)
            {
                var jT809_0x1001 = request.Package.Bodies as JT809_0x1001;
                SubordinateLinkClient.ConnectAsync(jT809_0x1001.DownLinkIP, jT809_0x1001.DownLinkPort, verifyCode);
            }
            return new JT809Response(package, 100);
        }

        /// <summary>
        /// 主链路注销请求消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns>主链路注销应答消息</returns>
        public virtual JT809Response Msg0x1003(JT809Request request)
        {
            var package = JT809BusinessType.主链路注销应答消息.Create();
            return new JT809Response(package, 100);
        }

        /// <summary>
        /// 主链路连接保持请求消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns>主链路连接保持应答消息</returns>
        public virtual JT809Response Msg0x1005(JT809Request request)
        {
            var package = JT809BusinessType.主链路连接保持应答消息.Create();
            return new JT809Response(package, 100);
        }

        /// <summary>
        /// 主链路动态信息交换消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual JT809Response Msg0x1200(JT809Request request)
        {

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns>从链路注销应答消息</returns>
        public virtual JT809Response Msg0x9004(JT809Request request)
        {
            return null;
        }
    }
}
