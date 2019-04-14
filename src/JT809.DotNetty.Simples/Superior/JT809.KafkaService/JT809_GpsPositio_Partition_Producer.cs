using Confluent.Kafka;
using Google.Protobuf;
using JT809.GrpcProtos;
using JT809.PubSub.Abstractions;
using Microsoft.Extensions.Options;

namespace JT809.KafkaService
{
    public sealed class JT809_GpsPositio_Partition_Producer : JT809PartitionProducer<JT809GpsPosition>
    {
        public JT809_GpsPositio_Partition_Producer(IOptions<JT809TopicOptions> topicOptionAccessor, IOptions<ProducerConfig> producerConfig, IJT809ProducerPartitionFactory producerPartitionFactory, IOptions<JT809PartitionOptions> partitionOptionsAccessor) : base(topicOptionAccessor, producerConfig, producerPartitionFactory, partitionOptionsAccessor)
        {
        }

        protected override Serializer<JT809GpsPosition> Serializer => (position) => position.ToByteArray();
    }
}
