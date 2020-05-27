using System;
using System.Collections.Generic;
using System.Text;
using Confluent.Kafka;
using JT809.KafkaService.Configs;
using JT809.PubSub.Abstractions;
using Microsoft.Extensions.Options;

namespace JT809.KafkaService
{
    public sealed class JT809_Same_Producer : IJT809Producer
    {
        private readonly JT809SameProducerConfig JT809SameProducerConfig;

        private IProducer<string, byte[]> Producer;

        private bool _disposed = false;

        public JT809_Same_Producer(IOptions<JT809SameProducerConfig> producerConfigAccessor)
        {
            JT809SameProducerConfig = producerConfigAccessor.Value;
            ProducerBuilder<string, byte[]> producerBuilder = new ProducerBuilder<string,byte[]>(producerConfigAccessor.Value);
            Producer= producerBuilder.Build();
            TopicName = JT809SameProducerConfig.Value.TopicName;
        }

        public string TopicName { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(true);
        }

        void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                Producer.Dispose();
            }
            _disposed = true;
        }

        public async void ProduceAsync(string msgId, string vno_color, byte[] data)
        {
            if (_disposed) return;
            await Producer.ProduceAsync(JT809SameProducerConfig.TopicName, new Message<string, byte[]>
            {
                Key = msgId,
                Value = data
            });
        }

        ~JT809_Same_Producer()
        {
            Dispose(false);
        }
    }
}
