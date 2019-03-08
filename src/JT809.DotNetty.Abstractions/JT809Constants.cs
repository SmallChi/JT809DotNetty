namespace JT809.DotNetty.Abstractions
{
    public static class JT809Constants
    {
        public const string SessionOnline= "JT809SessionOnline";

        public const string SessionOffline = "JT809SessionOffline";

        public static class JT809SuperiorWebApiRouteTable
        {
            public const string RouteTablePrefix = "/jt809api";

            public const string SessionPrefix = "Session";

            public const string SystemCollectPrefix = "SystemCollect";

            public const string HealthCheck = "HealthCheck";

            public const string Prefix = "Main";

            /// <summary>
            ///获取当前系统进程使用率
            /// </summary>
            public static string SystemCollectGet = $"{RouteTablePrefix}/{Prefix}/{SystemCollectPrefix}";

            /// <summary>
            ///健康检查
            /// </summary>
            public static string HealthCheckGet = $"{RouteTablePrefix}/{Prefix}/{HealthCheck}";
        }
    }
}
