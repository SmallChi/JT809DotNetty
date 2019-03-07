using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.DotNetty.Core.Configurations
{
    /// <summary>
    /// 下级平台配置项
    /// </summary>
    public class JT809InferiorPlatformOptions : IOptions<JT809InferiorPlatformOptions>
    {
        public JT809InferiorPlatformOptions Value => this;

        public int TcpPort { get; set; } = 809;

        public int QuietPeriodSeconds { get; set; } = 1;

        public TimeSpan QuietPeriodTimeSpan => TimeSpan.FromSeconds(QuietPeriodSeconds);

        public int ShutdownTimeoutSeconds { get; set; } = 3;

        public TimeSpan ShutdownTimeoutTimeSpan => TimeSpan.FromSeconds(ShutdownTimeoutSeconds);

        public int SoBacklog { get; set; } = 8192;

        public int EventLoopCount { get; set; } = Environment.ProcessorCount;
    }
}
