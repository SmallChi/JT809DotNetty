using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.KafkaService.Configs
{
    public class JT809SameConsumerConfig : ConsumerConfig, IOptions<JT809SameConsumerConfig>
    {
        public string TopicName { get; set; }

        public JT809SameConsumerConfig Value => this;
    }
}
