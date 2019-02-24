namespace JT809.DotNetty.Abstractions
{
    public static class JT809Constants
    {
        public const string SessionOnline= "JT809SessionOnline";

        public const string SessionOffline = "JT809SessionOffline";

        public static class JT809WebApiRouteTable
        {
            public const string RouteTablePrefix = "/jt809api";

            public const string SessionPrefix = "Session";

            public const string SystemCollectPrefix = "SystemCollect";

            public const string TcpPrefix = "Tcp";

            /// <summary>
            ///获取当前系统进程使用率
            /// </summary>
            public static string SystemCollectGet = $"{RouteTablePrefix}/{SystemCollectPrefix}/Get";        
        }
    }
}
