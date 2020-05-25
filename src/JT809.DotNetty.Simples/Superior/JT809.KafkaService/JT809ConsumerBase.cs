using Confluent.Kafka;
using JT809.KafkaService.Configs;
using JT809.PubSub.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JT809.KafkaService
{
    public abstract class JT809ConsumerBase<T> : IJT808ConsumerOfT<T>
    {
        public JT809ConsumerConfig ConsumerConfig { get; }

        protected JT809ConsumerBase(IOptions<JT809ConsumerConfig> config)
        {
            ConsumerConfig = config.Value;
        }

        public abstract CancellationTokenSource Cts { get; }
        protected abstract IConsumer<string, T> Consumer { get; }

        public abstract void Dispose();
        public abstract void OnMessage(Action<(string MsgId, T Data)> callback);
        public abstract void Subscribe();
        public abstract void Unsubscribe();
    }
}
