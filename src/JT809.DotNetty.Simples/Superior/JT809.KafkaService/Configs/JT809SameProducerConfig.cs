using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.KafkaService.Configs
{
    public class JT809SameProducerConfig : ProducerConfig,IOptions<JT809SameProducerConfig>
    {
        public string TopicName { get; set; }

        public JT809SameProducerConfig Value => this;
    }
}
