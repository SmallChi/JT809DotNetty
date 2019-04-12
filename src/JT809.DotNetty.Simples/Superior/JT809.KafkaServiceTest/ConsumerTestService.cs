using JT809.GrpcProtos;
using JT809.KafkaService;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.KafkaServiceTest
{
    public class ConsumerTestService
    {
        public JT809Consumer<byte[]> SameConsumer { get; }
        public JT809Consumer<JT809GpsPosition> GpsConsumer { get; }
        public ConsumerTestService(JT809Consumer<byte[]> sameConsumer, JT809Consumer<JT809GpsPosition> gpsConsumer)
        {
            SameConsumer = sameConsumer;
            GpsConsumer = gpsConsumer;
        }
    }
}
