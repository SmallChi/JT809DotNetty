using JT809.GrpcProtos;
using JT809.KafkaService;
using JT809.PubSub.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.KafkaServiceTest
{
    public class ProducerTestService
    {
        public JT809_Same_Producer SameProducer { get; }
        public JT809_GpsPositio_Producer GpsProducer { get; }
        public ProducerTestService(JT809_Same_Producer sameProducer, JT809_GpsPositio_Producer gpsProducer)
        {
            SameProducer = sameProducer;
            GpsProducer = gpsProducer;
        }

    }
}
