using Confluent.Kafka;
using Google.Protobuf;
using JT809.GrpcProtos;
using JT809.PubSub.Abstractions;
using Microsoft.Extensions.Options;

namespace JT809.KafkaService
{
    public sealed class JT809_GpsPositio_Producer : JT809Producer<JT809GpsPosition>
    {
        protected override IJT809ProducerPartitionFactory ProducerPartitionFactory { get; }

        protected override ProducerConfig ProducerConfig { get; }

        protected override JT809PartitionOptions PartitionOptions { get; }

        public JT809_GpsPositio_Producer(IOptions<ProducerConfig> producerConfigAccessor)
            : this(producerConfigAccessor,null, null )
        {

        }

        public JT809_GpsPositio_Producer(
            IOptions<ProducerConfig> producerConfigAccessor,
            IJT809ProducerPartitionFactory partitionFactory,
            IOptions<JT809PartitionOptions> partitionAccessor
          )
        {
            ProducerPartitionFactory = partitionFactory;
            ProducerConfig = producerConfigAccessor?.Value;
            PartitionOptions = partitionAccessor?.Value;
        }

        protected override IProducer<string, JT809GpsPosition> CreateProducer()
        {
            return new ProducerBuilder<string, JT809GpsPosition>(ProducerConfig)
                       .SetValueSerializer((position) => position.ToByteArray())
                       .Build();
        }
    }
}
