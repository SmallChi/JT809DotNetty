using JT809.GrpcProtos;
using JT809.KafkaService;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.KafkaServiceTest
{
    public class ProducerTestPartitionService
    {
        public JT809PartitionProducer<byte[]> SameProducer { get; }
        public JT809PartitionProducer<JT809GpsPosition> GpsProducer { get; }
        public ProducerTestPartitionService(JT809PartitionProducer<byte[]> sameProducer, JT809PartitionProducer<JT809GpsPosition> gpsProducer)
        {
            SameProducer = sameProducer;
            GpsProducer = gpsProducer;
        }

    }
}
