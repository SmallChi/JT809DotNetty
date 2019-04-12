using JT809.GrpcProtos;
using JT809.KafkaService;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.KafkaServiceTest
{
    public class ProducerTestService
    {
        public JT809Producer<byte[]> SameProducer { get; }
        public JT809Producer<JT809GpsPosition> GpsProducer { get; }
        public ProducerTestService(JT809Producer<byte[]> sameProducer, JT809Producer<JT809GpsPosition> gpsProducer)
        {
            SameProducer = sameProducer;
            GpsProducer = gpsProducer;
        }

    }
}
