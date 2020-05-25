using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.KafkaService.Configs
{
    public class JT809ProducerConfig : ProducerConfig,IOptions<JT809ProducerConfig>
    {
        public string TopicName { get; set; }

        public JT809ProducerConfig Value => this;
    }
}
