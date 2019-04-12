using Confluent.Kafka;
using Google.Protobuf;
using JT809.GrpcProtos;
using JT809.PubSub.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JT809.KafkaService
{
    public sealed class JT809_GpsPosition_Consumer : JT809Consumer<JT809GpsPosition>
    {
        public JT809_GpsPosition_Consumer(IOptions<JT809TopicOptions> topicOptionsAccessor, IOptions<ConsumerConfig> consumerConfigAccessor, ILoggerFactory loggerFactory) : base(topicOptionsAccessor, consumerConfigAccessor, loggerFactory)
        {
        }

        protected override Deserializer<JT809GpsPosition> Deserializer => (data, isNull) => {
            if (isNull) return default;
            return new MessageParser<JT809GpsPosition>(() => new JT809GpsPosition())
                   .ParseFrom(data.ToArray());
        };
    }
}
