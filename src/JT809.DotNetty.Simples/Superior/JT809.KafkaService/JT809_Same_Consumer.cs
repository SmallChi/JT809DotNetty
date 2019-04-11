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
    public sealed class JT809_Same_Consumer : JT809Consumer<byte[]>
    {
        protected override JT809PartitionOptions PartitionOptions { get; }

        public JT809_Same_Consumer(
            IOptions<ConsumerConfig> consumerConfigAccessor,
            ILoggerFactory loggerFactory,
            IOptions<JT809PartitionOptions> partitionAccessor          
            ) : base(consumerConfigAccessor.Value, loggerFactory)
        {
            PartitionOptions = partitionAccessor.Value;
        }

        public JT809_Same_Consumer(IOptions<ConsumerConfig> consumerConfigAccessor,ILoggerFactory loggerFactory) 
            : this(consumerConfigAccessor,loggerFactory, new JT809PartitionOptions())
        {
        }
    }
}
