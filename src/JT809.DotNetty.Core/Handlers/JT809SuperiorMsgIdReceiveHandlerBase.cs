using System;
using System.Collections.Generic;
using JT809.DotNetty.Core.Interfaces;
using JT809.DotNetty.Core.Metadata;
using JT809.Protocol.Enums;
using JT809.Protocol.Extensions;
using JT809.Protocol.MessageBody;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using JT809.Protocol;

namespace JT809.DotNetty.Core.Handlers
{
    /// <summary>
    /// 上级平台
    /// 主从链路接收消息处理程序
    /// </summary>
    public abstract class JT809SuperiorMsgIdReceiveHandlerBase
    {
        protected IJT809VerifyCodeGenerator VerifyCodeGenerator { get; }
        protected ILogger Logger { get; }
        protected IJT809SubordinateLoginService SubordinateLoginService { get; }

        /// <summary>
        /// 初始化消息处理业务
        /// </summary>
        protected JT809SuperiorMsgIdReceiveHandlerBase(
            ILoggerFactory loggerFactory,
            IJT809SubordinateLoginService jT809SubordinateLoginService,
            IJT809VerifyCodeGenerator verifyCodeGenerator)
        {
            this.Logger = loggerFactory.CreateLogger<JT809SuperiorMsgIdReceiveHandlerBase>();
            this.VerifyCodeGenerator = verifyCodeGenerator;
            this.SubordinateLoginService = jT809SubordinateLoginService;
            HandlerDict = new Dictionary<JT809BusinessType, Func<JT809Request, JT809Response>>
            {
                {JT809BusinessType.主链路登录请求消息, Msg0x1001},
                {JT809BusinessType.主链路注销请求消息, Msg0x1003},
                {JT809BusinessType.主链路连接保持请求消息, Msg0x1005},
                {JT809BusinessType.主链路断开通知消息,Msg0x1007 },
                {JT809BusinessType.主链路动态信息交换消息, Msg0x1200},
                {JT809BusinessType.下级平台主动关闭链路通知消息, Msg0x1008},

                {JT809BusinessType.从链路连接应答消息, Msg0x9002},
                {JT809BusinessType.从链路注销应答消息, Msg0x9004},
                {JT809BusinessType.从链路连接保持应答消息, Msg0x9006},
            };
            SubHandlerDict = new Dictionary<JT809SubBusinessType, Func<JT809Request, JT809Response>>
            {
                {JT809SubBusinessType.实时上传车辆定位信息, Msg0x1200_0x1202},
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
            Logger.LogInformation($"主链路登录请求消息:VerifyCode-{verifyCode}");
            var package = JT809BusinessType.主链路登录应答消息.Create(new JT809_0x1002()
            {
                Result = JT809_0x1002_Result.成功,
                VerifyCode = verifyCode
            });
            var jT809_0x1001 = request.Package.Bodies as JT809_0x1001;
            SubordinateLoginService.Login(jT809_0x1001.DownLinkIP, jT809_0x1001.DownLinkPort);
            return new JT809Response(package, 100);
        }

        /// <summary>
        /// 主链路注销请求消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns>主链路注销应答消息</returns>
        public virtual JT809Response Msg0x1003(JT809Request request)
        {
            var jT809_0x1003 = request.Package.Bodies as JT809_0x1003;
            Logger.LogInformation(JsonConvert.SerializeObject(jT809_0x1003));
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
        /// 主链路断开通知消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns>本条消息无需被通知方应答</returns>
        public virtual JT809Response Msg0x1007(JT809Request request)
        {
            var jT809_0x1007 = request.Package.Bodies as JT809_0x1007;
            Logger.LogInformation($"主链路断开通知消息:{jT809_0x1007.ErrorCode.ToString()}-{jT809_0x1007.ErrorCode}");
            return null;
        }

        /// <summary>
        /// 下级平台主动关闭链路通知消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns>本条消息无需被通知方应答</returns>
        public virtual JT809Response Msg0x1008(JT809Request request)
        {
            var jT809_0x1008 = request.Package.Bodies as JT809_0x1008;
            Logger.LogInformation($"下级平台主动关闭链路通知消息:{jT809_0x1008.ReasonCode.ToString()}-{jT809_0x1008.ReasonCode}");
            return null;
        }

        /// <summary>
        /// 从链路连接应答消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns>本条消息无需被通知方应答</returns>
        public virtual JT809Response Msg0x9002(JT809Request request)
        {
            var jT809_0x9002 = request.Package.Bodies as JT809_0x9002;
            Logger.LogInformation($"从链路连接应答消息:{jT809_0x9002.Result.ToString()}-{jT809_0x9002.Result}");
            return null;
        }

        /// <summary>
        ///  从链路注销应答消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns>本条消息无需被通知方应答</returns>
        public virtual JT809Response Msg0x9004(JT809Request request)
        {
            Logger.LogInformation("从链路注销应答消息");
            return null;
        }

        /// <summary>
        ///  从链路连接保持应答消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns>本条消息无需被通知方应答</returns>
        public virtual JT809Response Msg0x9006(JT809Request request)
        {
            Logger.LogInformation("从链路连接保持应答消息");
            return null;
        }

        /// <summary>
        /// 主链路动态信息交换消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual JT809Response Msg0x1200(JT809Request request)
        {
            var exchangeMessageBodies = request.Package.Bodies as JT809ExchangeMessageBodies;
            if (Logger.IsEnabled(LogLevel.Debug))
                Logger.LogDebug(JsonConvert.SerializeObject(request.Package));
            if (SubHandlerDict.TryGetValue(exchangeMessageBodies.SubBusinessType,out var func))
            {
                return func(request);
            }
            return null;
        }

        /// <summary>
        /// 实时上传车辆定位信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns>本条消息无需被通知方应答</returns>
        public virtual JT809Response Msg0x1200_0x1202(JT809Request request)
        {
            throw new NotImplementedException("实时上传车辆定位信息");
        }
    }
}
