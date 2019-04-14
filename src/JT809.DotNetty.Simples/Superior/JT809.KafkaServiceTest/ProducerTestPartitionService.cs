using JT809.GrpcProtos;
using JT809.KafkaService;
using JT809.PubSub.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.KafkaServiceTest
{
    public class ProducerTestPartitionService
    {
        public IJT809ProducerOfT<byte[]> SameProducer { get; }
        public IJT809ProducerOfT<JT809GpsPosition> GpsProducer { get; }
        public ProducerTestPartitionService(IJT809ProducerOfT<byte[]> sameProducer, IJT809ProducerOfT<JT809GpsPosition> gpsProducer)
        {
            SameProducer = sameProducer;
            GpsProducer = gpsProducer;
        }

    }
}
