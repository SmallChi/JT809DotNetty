using System;
using System.Collections.Generic;
using System.Text;
using Confluent.Kafka;
using JT809.PubSub.Abstractions;
using Microsoft.Extensions.Options;

namespace JT809.KafkaService
{
    public sealed class JT809_Same_Producer : JT809Producer<byte[]>
    {
        protected override IJT809ProducerPartitionFactory ProducerPartitionFactory { get; }

        protected override JT809PartitionOptions PartitionOptions { get; }

        public JT809_Same_Producer(IOptions<ProducerConfig> producerConfigAccessor)
            : this(producerConfigAccessor, null, null)
        {

        }

        public JT809_Same_Producer(
            IOptions<ProducerConfig> producerConfigAccessor,
            IJT809ProducerPartitionFactory partitionFactory,
            IOptions<JT809PartitionOptions> partitionAccessor
          ):base(producerConfigAccessor.Value)
        {
            ProducerPartitionFactory = partitionFactory;
            PartitionOptions = partitionAccessor?.Value;
        }
    }
}
