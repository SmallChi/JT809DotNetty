using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace JT809.PubSub.Abstractions
{
    public interface IJT809Consumer : IJT809PubSub, IJT808ConsumerOfT<byte[]>
    {

    }
    public interface IJT808ConsumerOfT<T> :IDisposable
    {
        void OnMessage(Action<(string MsgId, T Data)> callback);
        CancellationTokenSource Cts { get; }
        void Subscribe();
        void Unsubscribe();
    }
}
