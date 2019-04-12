using Confluent.Kafka;
using JT809.PubSub.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.KafkaService
{
    public abstract class JT809ProducerBase<T> : IJT809ProducerOfT<T>
    {
        protected JT809ProducerBase(string topicName,ProducerConfig config)
        {
            ProducerConfig = config;
            TopicName = topicName;
        }

        public ProducerConfig ProducerConfig { get;}
        public string TopicName { get; }
        protected abstract IProducer<string, T> Producer { get;}
        protected virtual Serializer<T> Serializer { get; set; }
        public abstract void Dispose();
        public abstract void ProduceAsync(string msgId, string vno_color, T data);
    }
}
