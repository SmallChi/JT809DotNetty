using System;
using System.Collections.Generic;
using JT809.DotNetty.Core.Metadata;
using JT809.Protocol.Enums;

namespace JT809.DotNetty.Core.Handlers
{
    /// <summary>
    /// 抽象从链路消息处理业务
    /// 自定义消息处理业务
    /// 注意:
    /// 1.ConfigureServices:
    /// services.Replace(new ServiceDescriptor(typeof(JT809SubordinateMsgIdTcpHandlerBase),typeof(JT809SubordinateMsgIdCustomTcpHandlerImpl),ServiceLifetime.Singleton));
    /// 2.解析具体的消息体，具体消息调用具体的JT809Serializer.Deserialize<T>
    /// </summary>
    public abstract class JT809SubordinateMsgIdHandlerBase
    {
        /// <summary>
        /// 初始化消息处理业务
        /// </summary>
        protected JT809SubordinateMsgIdHandlerBase()
        {
            HandlerDict = new Dictionary<JT809BusinessType, Func<JT809Request, JT809Response>>
            {
                {JT809BusinessType.从链路注销应答消息, Msg0x9004},
            };
        }

        public Dictionary<JT809BusinessType, Func<JT809Request, JT809Response>> HandlerDict { get; protected set; }

        /// <summary>
        /// 从链路注销应答消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual JT809Response Msg0x9004(JT809Request request)
        {
            return null;
        }
    }
}
