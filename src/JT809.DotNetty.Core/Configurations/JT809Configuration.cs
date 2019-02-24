using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.DotNetty.Core.Configurations
{
    public class JT809Configuration
    {
        public int TcpPort { get; set; } = 809;

        public int QuietPeriodSeconds { get; set; } = 1;

        public TimeSpan QuietPeriodTimeSpan => TimeSpan.FromSeconds(QuietPeriodSeconds);

        public int ShutdownTimeoutSeconds { get; set; } = 3;

        public TimeSpan ShutdownTimeoutTimeSpan => TimeSpan.FromSeconds(ShutdownTimeoutSeconds);

        public int SoBacklog { get; set; } = 8192;

        public int EventLoopCount { get; set; } = Environment.ProcessorCount;

        public int ReaderIdleTimeSeconds { get; set; } = 180;

        public int WriterIdleTimeSeconds { get; set; } = 60;

        public int AllIdleTimeSeconds { get; set; } = 180;
    }
}
