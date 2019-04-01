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

        public static IServiceCollection AddJT809Core(this IServiceCollection serviceDescriptors,IConfiguration configuration, Newtonsoft.Json.JsonSerializerSettings settings = null)
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
            serviceDescriptors.Configure<JT809Configuration>(configuration.GetSection("JT809Configuration"));
            serviceDescriptors.TryAddSingleton<JT809SimpleSystemCollectService>();
            //JT809计数器服务工厂
            serviceDescriptors.TryAddSingleton<JT809AtomicCounterServiceFactory>();
            //JT809编解码器
            serviceDescriptors.TryAddScoped<JT809Decoder>();
            serviceDescriptors.TryAddScoped<JT809Encoder>();
            return serviceDescriptors;
        }

        /// <summary>
        /// 下级平台
        /// 主链路为客户端
        /// 从链路为服务端
        /// </summary>
        /// <param name="serviceDescriptors"></param>
        /// <returns></returns>
        public static IServiceCollection AddJT809InferiorPlatform(this IServiceCollection  serviceDescriptors, Action<JT809InferiorPlatformOptions> options)
        {
            serviceDescriptors.Configure(options);
            //主从链路客户端和服务端连接处理器
            serviceDescriptors.TryAddScoped<JT809MainClientConnectionHandler>();
            serviceDescriptors.TryAddScoped<JT809SubordinateServerConnectionHandler>();
            //主链路服务端会话管理
            //serviceDescriptors.TryAddSingleton<JT809MainSessionManager>();
            //主从链路接收消息默认业务处理器
            serviceDescriptors.TryAddSingleton<JT809InferiorMsgIdReceiveHandlerBase, JT809InferiorMsgIdReceiveDefaultHandler>();
            //主从链路消息接收处理器
            serviceDescriptors.TryAddScoped<JT809MainServerHandler>();
            serviceDescriptors.TryAddScoped<JT809SubordinateServerHandler>();
            //主链路客户端
            serviceDescriptors.TryAddSingleton<JT809MainClient>();
            //从链路服务端
            serviceDescriptors.AddHostedService<JT809SubordinateServerHost>();
            return serviceDescriptors;
        }

        /// <summary>
        /// 上级平台
        /// 主链路为服务端
        /// 从链路为客户端
        /// </summary>
        /// <param name="serviceDescriptors"></param>
        /// <returns></returns>
        public static IServiceCollection AddJT809SuperiorPlatform(this IServiceCollection serviceDescriptors,Action<JT809SuperiorPlatformOptions> options)
        {
            serviceDescriptors.Configure(options);
            serviceDescriptors.TryAddSingleton<IJT809VerifyCodeGenerator, JT809VerifyCodeGeneratorDefaultImpl>();
            //主从链路客户端和服务端连接处理器
            serviceDescriptors.TryAddScoped<JT809MainServerConnectionHandler>();
            serviceDescriptors.TryAddScoped<JT809SubordinateClientConnectionHandler>();
            //主链路服务端会话管理
            serviceDescriptors.TryAddSingleton<JT809SuperiorMainSessionManager>();
            //主从链路接收消息默认业务处理器
            serviceDescriptors.TryAddSingleton<JT809SuperiorMsgIdReceiveHandlerBase, JT809SuperiorMsgIdReceiveDefaultHandler>();
            //主从链路消息接收处理器
            serviceDescriptors.TryAddScoped<JT809MainServerHandler>();
            serviceDescriptors.TryAddScoped<JT809SubordinateClientHandler>();
            serviceDescriptors.TryAddSingleton<IJT809SubordinateLoginService, JT809SubordinateLoginImplService>();
            serviceDescriptors.TryAddSingleton<IJT809SubordinateLinkNotifyService, JT809SubordinateLinkNotifyImplService>();
            //从链路客户端
            serviceDescriptors.TryAddSingleton<JT809SubordinateClient>();
            //主链路服务端
            serviceDescriptors.AddHostedService<JT809MainServerHost>();
            //上级平台webapi
            serviceDescriptors.TryAddSingleton<JT809SuperiorWebAPIHandlerBase, JT809SuperiorWebAPIDefaultHandler>();
            serviceDescriptors.TryAddScoped<JT809SuperiorWebAPIServerHandler>();
            serviceDescriptors.AddHostedService<JT809MainWebAPIServerHost>();
            return serviceDescriptors;
        }

        /// <summary>
        /// 上级平台
        /// 主链路为服务端
        /// 从链路为客户端
        /// </summary>
        /// <param name="serviceDescriptors"></param>
        /// <returns></returns>
        public static IServiceCollection AddJT809SuperiorPlatform(this IServiceCollection serviceDescriptors, IConfiguration superiorPlatformConfiguration)
        {
            serviceDescriptors.Configure<JT809SuperiorPlatformOptions>(superiorPlatformConfiguration.GetSection("JT809SuperiorPlatformConfiguration"));
            serviceDescriptors.TryAddSingleton<IJT809VerifyCodeGenerator, JT809VerifyCodeGeneratorDefaultImpl>();
            //主从链路客户端和服务端连接处理器
            serviceDescriptors.TryAddScoped<JT809MainServerConnectionHandler>();
            serviceDescriptors.TryAddScoped<JT809SubordinateClientConnectionHandler>();
            //主链路服务端会话管理
            serviceDescriptors.TryAddSingleton<JT809SuperiorMainSessionManager>();
            //主从链路接收消息默认业务处理器
            serviceDescriptors.TryAddSingleton<JT809SuperiorMsgIdReceiveHandlerBase, JT809SuperiorMsgIdReceiveDefaultHandler>();
            //主从链路消息接收处理器
            serviceDescriptors.TryAddScoped<JT809MainServerHandler>();
            serviceDescriptors.TryAddScoped<JT809SubordinateClientHandler>();
            serviceDescriptors.TryAddSingleton<IJT809SubordinateLoginService, JT809SubordinateLoginImplService>();
            serviceDescriptors.TryAddSingleton<IJT809SubordinateLinkNotifyService, JT809SubordinateLinkNotifyImplService>();
            //从链路客户端
            serviceDescriptors.TryAddSingleton<JT809SubordinateClient>();
            //主链路服务端
            serviceDescriptors.AddHostedService<JT809MainServerHost>();
            //上级平台webapi
            serviceDescriptors.TryAddSingleton<JT809SuperiorWebAPIHandlerBase, JT809SuperiorWebAPIDefaultHandler>();
            serviceDescriptors.TryAddScoped<JT809SuperiorWebAPIServerHandler>();
            serviceDescriptors.AddHostedService<JT809MainWebAPIServerHost>();
            return serviceDescriptors;
        }
    }
}