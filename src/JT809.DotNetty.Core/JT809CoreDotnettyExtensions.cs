using JT809.DotNetty.Abstractions;
using JT809.DotNetty.Core.Configurations;
using JT809.DotNetty.Core.Converters;
using JT809.DotNetty.Core.Interfaces;
using JT809.DotNetty.Core.Internal;
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
            serviceDescriptors.TryAddSingleton<IVerifyCodeGenerator, VerifyCodeGeneratorDefaultImpl>();
            serviceDescriptors.TryAddSingleton<IJT809SessionPublishing, JT809SessionPublishingEmptyImpl>();
            serviceDescriptors.TryAddSingleton<JT809SimpleSystemCollectService>();
            return serviceDescriptors;
        }
    }
}