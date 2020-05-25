using Confluent.Kafka;
using JT809.PubSub.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.KafkaService
{
    public abstract class JT809ProducerBase<T> : IJT809ProducerOfT<T>
    {
        protected JT809ProducerBase(ProducerConfig config)
        {
            ProducerConfig = config;
        }

        public ProducerConfig ProducerConfig { get;}
        protected abstract IProducer<string, T> Producer { get;}
        public abstract void Dispose();
        public abstract void ProduceAsync(string msgId, string vno_color, T data);
    }
}
