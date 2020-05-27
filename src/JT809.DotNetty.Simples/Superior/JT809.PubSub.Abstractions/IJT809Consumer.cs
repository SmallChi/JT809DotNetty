using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace JT809.PubSub.Abstractions
{
    public interface IJT809Consumer : IJT809PubSub
    {
        void OnMessage(Action<(string MsgId, byte[] Data)> callback);
        CancellationTokenSource Cts { get; }
        void Subscribe();
        void Unsubscribe();
    }
}
