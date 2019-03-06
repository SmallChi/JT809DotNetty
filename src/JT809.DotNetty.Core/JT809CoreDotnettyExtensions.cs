using JT809.DotNetty.Abstractions;
using JT809.DotNetty.Core.Codecs;
using JT809.DotNetty.Core.Configurations;
using JT809.DotNetty.Core.Converters;
using JT809.DotNetty.Core.Enums;
using JT809.DotNetty.Core.Handlers;
using JT809.DotNetty.Core.Interfaces;
using JT809.DotNetty.Core.Internal;
using JT809.DotNetty.Core.Links;
using JT809.DotNetty.Core.Metadata;
using JT809.DotNetty.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("JT809.DotNetty.Core.Test")]
[assembly: InternalsVisibleTo("JT809.DotNetty.Tcp.Test")]
[assembly: InternalsVisibleTo("JT809.DotNetty.Udp.Test")]
[assembly: InternalsVisibleTo("JT809.DotNetty.WebApi.Test")]

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

        public static IServiceCollection AddJT809Core(this IServiceCollection  serviceDescriptors, IConfiguration configuration, Newtonsoft.Json.JsonSerializerSettings settings=null)
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
            serviceDescriptors.TryAddSingleton<IJT809VerifyCodeGenerator, JT809VerifyCodeGeneratorDefaultImpl>();
            serviceDescriptors.TryAddSingleton<JT809AtomicCounterServiceFactory>();
            //JT809编解码器
            serviceDescriptors.TryAddScoped<JT809Decoder>();
            serviceDescriptors.TryAddScoped<JT809Encoder>();
            //主从链路连接处理器
            serviceDescriptors.TryAddScoped<JT809SubordinateConnectionHandler>();
            serviceDescriptors.TryAddScoped<JT809MainServerConnectionHandler>();
            //从链路客户端
            serviceDescriptors.TryAddSingleton<JT809SubordinateClient>();
            //主从链路消息默认业务处理器实现
            serviceDescriptors.TryAddSingleton<JT809MainMsgIdHandlerBase, JT809MainMsgIdDefaultHandler>();
            serviceDescriptors.TryAddSingleton<JT809SubordinateMsgIdHandlerBase, JT809SubordinateMsgIdDefaultHandler>();
            //主从链路消息接收处理器
            serviceDescriptors.TryAddScoped<JT809MainServerHandler>();
            serviceDescriptors.TryAddScoped<JT809SubordinateServerHandler>();
            //主链路服务端
            serviceDescriptors.AddHostedService<JT809MainServerHost>();
            return serviceDescriptors;
        }
    }
}