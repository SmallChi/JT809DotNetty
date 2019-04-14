using JT809.GrpcProtos;
using JT809.KafkaService;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.KafkaServiceTest
{
    public class ConsumerTestPartitionService
    {
        public JT809PartitionConsumer<byte[]> SameConsumer { get; }
        public JT809PartitionConsumer<JT809GpsPosition> GpsConsumer { get; }
        public ConsumerTestPartitionService(JT809PartitionConsumer<byte[]> sameConsumer, JT809PartitionConsumer<JT809GpsPosition> gpsConsumer)
        {
            SameConsumer = sameConsumer;
            GpsConsumer = gpsConsumer;
        }
    }
}
