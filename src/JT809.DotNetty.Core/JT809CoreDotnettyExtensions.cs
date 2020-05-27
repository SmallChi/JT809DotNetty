using JT809.DotNetty.Abstractions;
using JT809.DotNetty.Core.Codecs;
using JT809.DotNetty.Core.Configurations;
using JT809.DotNetty.Core.Converters;
using JT809.DotNetty.Core.Enums;
using JT809.DotNetty.Core.Handlers;
using JT809.DotNetty.Core.Interfaces;
using JT809.DotNetty.Core.Internal;
using JT809.DotNetty.Core.Clients;
using JT809.DotNetty.Core.Metadata;
using JT809.DotNetty.Core.Services;
using JT809.DotNetty.Core.Servers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using JT808.DotNetty.WebApi;
using JT809.DotNetty.Core.Session;
using JT809.DotNetty.Core.Events;
using JT809.Protocol;

[assembly: InternalsVisibleTo("JT809.DotNetty.Core.Test")]

namespace JT809.DotNetty.Core
{
    public static class JT809CoreDotnettyExtensions
    {
        static JT809CoreDotnettyExtensions()
        {
            JsonConvert.DefaultSettings = new Func<JsonSerializerSettings>(() =>
            {
                Newtonsoft.Json.JsonSerializerSettings settings = new Newtonsoft.Json.JsonSerializerSettings();
                //日期类型默认格式化处理
                settings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.MicrosoftDateFormat;
                settings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                //空值处理
                settings.NullValueHandling = NullValueHandling.Ignore;
                settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                settings.Converters.Add(new JsonIPAddressConverter());
                settings.Converters.Add(new JsonIPEndPointConverter());
                return settings;
            });
        }

        public static IJT809Builder AddJT809Core(this IJT809Builder builder,IConfiguration configuration, Newtonsoft.Json.JsonSerializerSettings settings = null)
        {
            if (settings != null)
            {
                JsonConvert.DefaultSettings = new Func<JsonSerializerSettings>(() =>
                {
                    settings.Converters.Add(new JsonIPAddressConverter());
                    settings.Converters.Add(new JsonIPEndPointConverter());
                    settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    return settings;
                });
            }
            builder.Services.Configure<JT809Configuration>(configuration.GetSection("JT809Configuration"));
            builder.Services.TryAddSingleton<JT809SimpleSystemCollectService>();
            //JT809计数器服务工厂
            builder.Services.TryAddSingleton<JT809AtomicCounterServiceFactory>();
            //JT809编解码器
            builder.Services.TryAddScoped<JT809Decoder>();
            builder.Services.TryAddScoped<JT809Encoder>();
            return builder;
        }

        /// <summary>
        /// 下级平台
        /// 主链路为客户端
        /// 从链路为服务端
        /// </summary>
        /// <param name="serviceDescriptors"></param>
        /// <returns></returns>
        public static IJT809Builder AddJT809InferiorPlatform(this IJT809Builder builder, Action<JT809InferiorPlatformOptions> options)
        {
            builder.Services.Configure(options);
            //主从链路客户端和服务端连接处理器
            builder.Services.TryAddScoped<JT809MainClientConnectionHandler>();
            builder.Services.TryAddScoped<JT809SubordinateServerConnectionHandler>();
            //主链路服务端会话管理
            //serviceDescriptors.TryAddSingleton<JT809MainSessionManager>();
            //主从链路接收消息默认业务处理器
            builder.Services.TryAddSingleton<JT809InferiorMsgIdReceiveHandlerBase, JT809InferiorMsgIdReceiveDefaultHandler>();
            //主从链路消息接收处理器
            builder.Services.TryAddScoped<JT809SubordinateServerHandler>();
            //主链路客户端
            //serviceDescriptors.TryAddSingleton<JT809MainClient>();
            //从链路服务端
            builder.Services.AddHostedService<JT809SubordinateServerHost>();
            return builder;
        }

        /// <summary>
        /// 下级平台
        /// 主链路为客户端
        /// 从链路为服务端
        /// </summary>
        /// <param name="serviceDescriptors"></param>
        /// <returns></returns>
        public static IJT809Builder AddJT809InferiorPlatformClient(this IJT809Builder builder)
        {
            builder.Services.TryAddSingleton<IJT809ManualResetEvent, JT809InferoprManualResetEvent>();
            //主从链路客户端和服务端连接处理器
            builder.Services.TryAddScoped<JT809MainClientConnectionHandler>();
            //主从链路接收消息默认业务处理器
            builder.Services.TryAddSingleton<JT809InferiorMsgIdReceiveHandlerBase, JT809InferiorMsgIdReceiveDefaultHandler>();
            //主从链路消息接收处理器
            builder.Services.TryAddScoped<JT809MainClientHandler>();
            //主链路客户端
            builder.Services.TryAddSingleton<JT809MainClient>();
            return builder;
        }


        /// <summary>
        /// 上级平台
        /// 主链路为服务端
        /// 从链路为客户端
        /// </summary>
        /// <param name="serviceDescriptors"></param>
        /// <returns></returns>
        public static IJT809Builder AddJT809SuperiorPlatform(this IJT809Builder builder, IConfiguration superiorPlatformConfiguration=null, Action<JT809SuperiorPlatformOptions> options=null)
        {
            if (superiorPlatformConfiguration != null)
            {
                builder.Services.Configure<JT809SuperiorPlatformOptions>(superiorPlatformConfiguration.GetSection("JT809SuperiorPlatformConfiguration"));
            }
            if (options != null)
            {
                builder.Services.Configure(options);
            }
            builder.Services.TryAddSingleton<IJT809VerifyCodeGenerator, JT809VerifyCodeGeneratorDefaultImpl>();
            //主从链路客户端和服务端连接处理器
            builder.Services.TryAddScoped<JT809MainServerConnectionHandler>();
            builder.Services.TryAddScoped<JT809SubordinateClientConnectionHandler>();
            //主链路服务端会话管理
            builder.Services.TryAddSingleton<JT809SuperiorMainSessionManager>();
            //主从链路接收消息默认业务处理器
            builder.Services.TryAddSingleton<JT809SuperiorMsgIdReceiveHandlerBase, JT809SuperiorMsgIdReceiveDefaultHandler>();
            //主从链路消息接收处理器
            builder.Services.TryAddScoped<JT809MainServerHandler>();
            builder.Services.TryAddScoped<JT809SubordinateClientHandler>();
            builder.Services.TryAddSingleton<IJT809SubordinateLoginService, JT809SubordinateLoginImplService>();
            builder.Services.TryAddSingleton<IJT809SubordinateLinkNotifyService, JT809SubordinateLinkNotifyImplService>();
            //从链路客户端
            builder.Services.TryAddSingleton<JT809SubordinateClient>();
            //主链路服务端
            builder.Services.AddHostedService<JT809MainServerHost>();
            //上级平台webapi
            builder.Services.TryAddSingleton<JT809SuperiorWebAPIHandlerBase, JT809SuperiorWebAPIDefaultHandler>();
            builder.Services.TryAddScoped<JT809SuperiorWebAPIServerHandler>();
            builder.Services.AddHostedService<JT809MainWebAPIServerHost>();
            return builder;
        }
    }
}