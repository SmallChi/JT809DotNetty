using Confluent.Kafka;
using Google.Protobuf;
using JT809.GrpcProtos;
using JT809.KafkaService.Configs;
using JT809.PubSub.Abstractions;
using Microsoft.Extensions.Options;

namespace JT809.KafkaService
{
    public sealed class JT809_GpsPositio_Producer : JT809Producer<JT809GpsPosition>
    {
        public JT809_GpsPositio_Producer(IOptions<JT809ProducerConfig> producerConfigAccessor) : base( producerConfigAccessor)
        {
        }
    }
}
