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
        public IJT808ConsumerOfT<byte[]> SameConsumer { get; }
        public IJT808ConsumerOfT<JT809GpsPosition> GpsConsumer { get; }
        public ConsumerTestService(IJT808ConsumerOfT<byte[]> sameConsumer, IJT808ConsumerOfT<JT809GpsPosition> gpsConsumer)
        {
            SameConsumer = sameConsumer;
            GpsConsumer = gpsConsumer;
        }
    }
}
