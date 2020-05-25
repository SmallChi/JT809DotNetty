using Confluent.Kafka;
using Google.Protobuf;
using JT809.GrpcProtos;
using JT809.KafkaService.Configs;
using JT809.PubSub.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JT809.KafkaService
{
    public sealed class JT809_Same_Consumer : JT809Consumer<byte[]>
    {
        public JT809_Same_Consumer(IOptions<JT809ConsumerConfig> consumerConfigAccessor, ILoggerFactory loggerFactory) 
            : base(consumerConfigAccessor, loggerFactory)
        {
        }
    }
}
