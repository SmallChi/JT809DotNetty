using Confluent.Kafka;
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
        public string TopicName { get; }

        public ConsumerConfig ConsumerConfig { get; }

        protected JT809ConsumerBase(string topicName, ConsumerConfig config)
        {
            ConsumerConfig = config;
            TopicName = topicName;
        }

        public abstract CancellationTokenSource Cts { get; }
        protected abstract IList<IConsumer<string, T>> Consumers { get; }

        protected virtual Deserializer<T> Deserializer { get; set; }

        public abstract void Dispose();
        public abstract void OnMessage(Action<(string MsgId, T Data)> callback);
        public abstract void Subscribe();
        public abstract void Unsubscribe();
    }
}
