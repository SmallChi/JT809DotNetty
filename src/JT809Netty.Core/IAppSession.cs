using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace JT809Netty.Core
{
    public interface IAppSession
    {
        string SessionID { get; }

        IChannel Channel { get; }

        DateTime LastActiveTime { get; set; }

        DateTime StartTime { get; }
    }
}
