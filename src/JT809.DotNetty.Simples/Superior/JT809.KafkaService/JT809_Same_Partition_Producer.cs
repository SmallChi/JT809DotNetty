using System;
using System.Collections.Generic;
using System.Text;
using Confluent.Kafka;
using JT809.PubSub.Abstractions;
using Microsoft.Extensions.Options;

namespace JT809.KafkaService
{
    public sealed class JT809_Same_Partition_Producer : JT809PartitionProducer<byte[]>
    {
        public JT809_Same_Partition_Producer(IOptions<JT809TopicOptions> topicOptionAccessor, IOptions<ProducerConfig> producerConfig, IJT809ProducerPartitionFactory producerPartitionFactory, IOptions<JT809PartitionOptions> partitionOptionsAccessor) : base(topicOptionAccessor, producerConfig, producerPartitionFactory, partitionOptionsAccessor)
        {
        }
    }
}
