using JT809.PubSub.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.KafkaService.Partitions
{
    public class JT809GpsPositionProducerPartitionFactoryImpl : IJT809ProducerPartitionFactory
    {
        private readonly JT809PartitionOptions partition;

        public JT809GpsPositionProducerPartitionFactoryImpl(IOptions<JT809PartitionOptions> partitionAccessor)
        {
            partition = partitionAccessor.Value;
        }

        public int CreatePartition(string topicName, string msgId, string vno_color)
        {
            var key1Byte1 = JT809HashAlgorithm.ComputeMd5(vno_color);
            var p = JT809HashAlgorithm.Hash(key1Byte1, 2) % partition.Partition;
            return (int)p;
        }
    }
}
