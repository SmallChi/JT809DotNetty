using System;
using System.Collections.Generic;
using JT809.DotNetty.Core.Interfaces;
using JT809.DotNetty.Core.Metadata;
using JT809.Protocol.Enums;
using JT809.Protocol.Extensions;
using JT809.Protocol.MessageBody;

namespace JT809.DotNetty.Core.Handlers
{
    /// <summary>
    /// 基于Tcp模式抽象消息处理业务
    /// 自定义消息处理业务
    /// 注意:
    /// 1.ConfigureServices:
    /// services.Replace(new ServiceDescriptor(typeof(JT809MsgIdTcpHandlerBase),typeof(JT809MsgIdCustomTcpHandlerImpl),ServiceLifetime.Singleton));
    /// 2.解析具体的消息体，具体消息调用具体的JT809Serializer.Deserialize<T>
    /// </summary>
    public abstract class JT809MsgIdTcpHandlerBase
    {
        protected JT809TcpSessionManager sessionManager { get; }
        protected IVerifyCodeGenerator verifyCodeGenerator { get; }
        /// <summary>
        /// 初始化消息处理业务
        /// </summary>
        protected JT809MsgIdTcpHandlerBase(
            IVerifyCodeGenerator verifyCodeGenerator,
            JT809TcpSessionManager sessionManager)
        {
            this.sessionManager = sessionManager;
            this.verifyCodeGenerator = verifyCodeGenerator;
            HandlerDict = new Dictionary<JT809BusinessType, Func<JT809Request, JT809Response>>
            {
                {JT809BusinessType.主链路登录请求消息, Msg0x1001},
                {JT809BusinessType.主链路注销请求消息, Msg0x1003},
                {JT809BusinessType.主链路连接保持请求消息, Msg0x1005},
                {JT809BusinessType.主链路动态信息交换消息, Msg0x1200}
            };
            //SubHandlerDict = new Dictionary<JT809SubBusinessType, Func<JT809Request, JT809Response>>
            //{
            //    {JT809SubBusinessType.实时上传车辆定位信息, Msg0x1200_0x1202},
            //};
        }

        public Dictionary<JT809BusinessType, Func<JT809Request, JT809Response>> HandlerDict { get; protected set; }

        //public Dictionary<JT809SubBusinessType, Func<JT809Request, JT809Response>> SubHandlerDict { get; protected set; }

        /// <summary>
        /// 主链路登录应答消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual JT809Response Msg0x1001(JT809Request request)
        {
            var package= JT809BusinessType.主链路登录应答消息.Create(new JT809_0x1002() 
            {
                 Result= JT809_0x1002_Result.成功,
                 VerifyCode= verifyCodeGenerator.Create()
            });

            return new JT809Response(package,100);
        }

        /// <summary>
        /// 主链路注销应答消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual JT809Response Msg0x1003(JT809Request request)
        {
            var package = JT809BusinessType.主链路注销应答消息.Create();
            return new JT809Response(package, 100);
        }

        /// <summary>
        /// 主链路连接保持应答消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
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

        ///// <summary>
        ///// 主链路动态信息交换消息 
        ///// </summary>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //public virtual JT809Response Msg0x1200_0x1202(JT809Request request)
        //{

        //    return null;
        //}
    }
}
