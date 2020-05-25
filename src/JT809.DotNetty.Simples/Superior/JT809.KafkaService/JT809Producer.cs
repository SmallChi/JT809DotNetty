using Confluent.Kafka;
using Confluent.Kafka.Admin;
using JT809.KafkaService.Configs;
using JT809.PubSub.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace JT809.KafkaService
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class JT809Producer<T> : JT809ProducerBase<T>
    {
        private bool _disposed = false;

        protected virtual IProducer<string, T> CreateProducer()
        {
            ProducerBuilder<string, T> producerBuilder = new ProducerBuilder<string, T>(ProducerConfig);
            return producerBuilder.Build();
        }

        protected override IProducer<string, T> Producer { get; }

        protected JT809Producer(
            IOptions<JT809ProducerConfig> producerConfigAccessor)
            : base(producerConfigAccessor.Value)
        {
            Producer = CreateProducer();
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(true);

        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                Producer.Dispose();
            }
            _disposed = true;
        }

        public override async void ProduceAsync(string msgId, string vno_color, T data)
        {
            if (_disposed) return;
            await Producer.ProduceAsync(ProducerConfig.TopicName, new Message<string, T>
            {
                Key = msgId,
                Value = data
            });
        }

        ~JT809Producer()
        {
            Dispose(false);
        }
    }
}
