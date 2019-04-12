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
        public JT809_Same_Producer(IOptions<JT809TopicOptions> topicOptionAccessor, IOptions<ProducerConfig> producerConfigAccessor)
            : base(topicOptionAccessor, producerConfigAccessor)
        {
        }
    }
}
