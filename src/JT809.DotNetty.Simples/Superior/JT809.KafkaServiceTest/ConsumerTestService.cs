using JT809.GrpcProtos;
using JT809.KafkaService;
using JT809.PubSub.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.KafkaServiceTest
{
    public class ConsumerTestService
    {
        public JT809_Same_Consumer SameConsumer { get; }
        public JT809_GpsPosition_Consumer GpsConsumer { get; }
        public ConsumerTestService(JT809_Same_Consumer sameConsumer, JT809_GpsPosition_Consumer gpsConsumer)
        {
            SameConsumer = sameConsumer;
            GpsConsumer = gpsConsumer;
        }
    }
}
