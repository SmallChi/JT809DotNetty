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
using Newtonsoft.Json;

namespace JT809.DotNetty.Core.Handlers
{
    /// <summary>
    /// 下级平台
    /// 主从链路接收消息处理程序
    /// </summary>
    public abstract class JT809InferiorMsgIdReceiveHandlerBase
    {
        protected JT809Configuration  Configuration { get; }
        protected ILogger Logger { get; }
        protected IJT809ManualResetEvent ManualResetEvent { get; }

        /// <summary>
        /// 初始化消息处理业务
        /// </summary>
        protected JT809InferiorMsgIdReceiveHandlerBase(
            IJT809ManualResetEvent jT809ManualResetEvent,
            ILoggerFactory loggerFactory)
        {
            this.Logger = loggerFactory.CreateLogger<JT809SuperiorMsgIdReceiveHandlerBase>();
            this.ManualResetEvent = jT809ManualResetEvent;
            HandlerDict = new Dictionary<ushort, Func<JT809Request, JT809Response>>
            {
                {JT809BusinessType.主链路登录应答消息.ToUInt16Value(),Msg0x1002},
                {JT809BusinessType.主链路连接保持应答消息.ToUInt16Value(),Msg0x1006},
                {JT809BusinessType.从链路连接请求消息.ToUInt16Value(),Msg0x9001},
                {JT809BusinessType.从链路注销请求消息.ToUInt16Value(), Msg0x9003},
                {JT809BusinessType.从链路连接保持请求消息.ToUInt16Value(),Msg0x9005 }, 
            };
            SubHandlerDict = new Dictionary<ushort, Func<JT809Request, JT809Response>>
            {
                //{JT809SubBusinessType.实时上传车辆定位信息, Msg0x1200_0x1202},
            };
        }

        public Dictionary<ushort, Func<JT809Request, JT809Response>> HandlerDict { get; protected set; }

        public Dictionary<ushort, Func<JT809Request, JT809Response>> SubHandlerDict { get; protected set; }


        /// <summary>
        /// 主链路登录应答消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual JT809Response Msg0x1002(JT809Request request)
        {
            if (Logger.IsEnabled(LogLevel.Information))
            {
                Logger.LogInformation(JsonConvert.SerializeObject(request));
            }
            var jT809_0x1002 = request.Package.Bodies as JT809_0x1002;
            if(jT809_0x1002.Result== JT809_0x1002_Result.成功)
            {
                ManualResetEvent.Resume();
            }
            return null;
        }
        /// <summary>
        /// 主链路连接保持应答消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual JT809Response Msg0x1006(JT809Request request)
        {
            if (Logger.IsEnabled(LogLevel.Information))
            {
                Logger.LogInformation(JsonConvert.SerializeObject(request));
            }
            return null;
        }
        /// <summary>
        /// 从链路连接请求消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns>从链路连接应答消息</returns>
        public virtual JT809Response Msg0x9001(JT809Request request)
        {
            var package = JT809BusinessType.从链路连接应答信息.Create(new JT809_0x9002 {
                Result = JT809_0x9002_Result.成功
            });
            if (Logger.IsEnabled(LogLevel.Information))
            {
                Logger.LogInformation(JsonConvert.SerializeObject(request));
            }
            return new JT809Response(package, 100);
        }

        /// <summary>
        /// 从链路注销请求消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns>从链路注销应答消息</returns>
        public virtual JT809Response Msg0x9003(JT809Request request)
        {
            var package = JT809BusinessType.从链路注销应答消息.Create();
            if (Logger.IsEnabled(LogLevel.Information))
            {
                Logger.LogInformation(JsonConvert.SerializeObject(request));
            }
            return new JT809Response(package, 100);
        }

        /// <summary>
        /// 从链路连接保持请求消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns>从链路连接保持应答消息</returns>
        public virtual JT809Response Msg0x9005(JT809Request request)
        {
            var package = JT809BusinessType.从链路连接保持应答消息.Create();
            if (Logger.IsEnabled(LogLevel.Information))
            {
                Logger.LogInformation(JsonConvert.SerializeObject(request));
            }
            return new JT809Response(package, 100);
        }
    }
}
