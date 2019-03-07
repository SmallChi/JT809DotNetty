using JT809.DotNetty.Core;
using JT809.DotNetty.Core.Codecs;
using JT809.DotNetty.Core.Handlers;
using JT809.DotNetty.Core.Services;
using JT809.DotNetty.Tcp.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("JT809.DotNetty.Tcp.Test")]

namespace JT809.DotNetty.Tcp
{
    public static class JT809TcpDotnettyExtensions
    {
        public static IServiceCollection AddJT809TcpHost(this IServiceCollection  serviceDescriptors)
        {
            serviceDescriptors.TryAddSingleton<JT809MainSessionManager>();
            serviceDescriptors.TryAddSingleton<JT809MainMsgIdHandlerBase, JT809MsgIdDefaultTcpHandler>();
            serviceDescriptors.TryAddScoped<JT809TcpConnectionHandler>();
            serviceDescriptors.TryAddScoped<JT809Decoder>();
            serviceDescriptors.TryAddScoped<JT809TcpServerHandler>();
            serviceDescriptors.AddHostedService<JT809TcpServerHost>();
            return serviceDescriptors;
        }
    }
}