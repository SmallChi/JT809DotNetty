using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.KafkaService.Configs
{
    public class JT809ConsumerConfig: ConsumerConfig, IOptions<JT809ConsumerConfig>
    {
        public string TopicName { get; set; }

        public JT809ConsumerConfig Value => this;
    }
}
