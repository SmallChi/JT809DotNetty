using Confluent.Kafka;
using Google.Protobuf;
using JT809.GrpcProtos;
using JT809.KafkaService.Configs;
using JT809.PubSub.Abstractions;
using Microsoft.Extensions.Options;
using System;

namespace JT809.KafkaService
{
    public sealed class JT809_GpsPositio_Producer : IJT809Producer
    {
        private readonly JT809ProducerConfig JT809ProducerConfig;

        private IProducer<string, byte[]> Producer;

        private bool _disposed = false;

        public JT809_GpsPositio_Producer(IOptions<JT809ProducerConfig> producerConfigAccessor)
        {
            JT809ProducerConfig = producerConfigAccessor.Value;
            ProducerBuilder<string, byte[]> producerBuilder = new ProducerBuilder<string, byte[]>(producerConfigAccessor.Value);
            Producer = producerBuilder.Build();
            TopicName = JT809ProducerConfig.TopicName;
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
            await Producer.ProduceAsync(JT809ProducerConfig.TopicName, new Message<string, byte[]>
            {
                Key = msgId,
                Value = data
            });
        }

        ~JT809_GpsPositio_Producer()
        {
            Dispose(false);
        }
    }
}
