using System;
using System.Collections.Generic;
using System.Text;
using JT809.DotNetty.Abstractions.Dtos;
using JT809.DotNetty.Core.Metadata;
using Newtonsoft.Json;

namespace JT809.DotNetty.Core.Handlers
{
    /// <summary>
    /// 上级平台
    /// 基于webapi http模式抽象消息处理业务
    /// 自定义消息处理业务
    /// 注意:
    /// 1.ConfigureServices:
    /// services.Replace(new ServiceDescriptor(typeof(JT809SuperiorWebAPIHandlerBase),typeof(JT809SuperiorWebAPICustomHandler),ServiceLifetime.Singleton));
    /// 2.解析具体的消息体，具体消息调用具体的JT809Serializer.Deserialize<T>
    /// </summary>
    public abstract class JT809SuperiorWebAPIHandlerBase
    {
        /// <summary>
        /// 初始化消息处理业务
        /// </summary>
        protected JT809SuperiorWebAPIHandlerBase()
        {
            HandlerDict = new Dictionary<string, Func<JT809HttpRequest, JT809HttpResponse>>();
        }

        protected void CreateRoute(string url, Func<JT809HttpRequest, JT809HttpResponse> func)
        {
            if (!HandlerDict.ContainsKey(url))
            {
                HandlerDict.Add(url, func);
            }
            else
            {
                // 替换
                HandlerDict[url] = func;
            }
        }

        public Dictionary<string, Func<JT809HttpRequest, JT809HttpResponse>> HandlerDict { get; }

        protected JT809HttpResponse CreateJT809HttpResponse(dynamic dynamicObject)
        {
            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dynamicObject));
            return new JT809HttpResponse()
            {
                Data = data
            };
        }

        public JT809HttpResponse DefaultHttpResponse()
        {
            byte[] json = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new JT809DefaultResultDto()));
            return new JT809HttpResponse(json);
        }

        public JT809HttpResponse EmptyHttpResponse()
        {
            byte[] json = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new JT809ResultDto<string>()
            {
                Code = JT809ResultCode.Empty,
                Message = "内容为空",
                Data = "Content Empty"
            }));
            return new JT809HttpResponse(json);
        }

        public JT809HttpResponse NotFoundHttpResponse()
        {
            byte[] json = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new JT809ResultDto<string>()
            {
                Code = JT809ResultCode.NotFound,
                Message = "没有该服务",
                Data = "没有该服务"
            }));
            return new JT809HttpResponse(json);
        }

        public JT809HttpResponse ErrorHttpResponse(Exception ex)
        {
            byte[] json = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new JT809ResultDto<string>()
            {
                Code = JT809ResultCode.Error,
                Message = JsonConvert.SerializeObject(ex),
                Data = ex.Message
            }));
            return new JT809HttpResponse(json);
        }
    }
}
