using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using JT809.Protocol.Enums;
using JT809.Protocol.Extensions;

namespace JT809.KafkaServiceTest
{
    public class ProducerPartitionTest : TestProducerPartitionBase
    {
        [Fact]
        public void Test1()
        {
            ProducerTestPartitionService producerTestService = ServiceProvider.GetRequiredService<ProducerTestPartitionService>();
            producerTestService.GpsProducer.ProduceAsync(JT809SubBusinessType.实时上传车辆定位信息.ToValueString(), "粤A23456_2", new GrpcProtos.JT809GpsPosition
            {
                 Vno= "粤A23456",
                 VColor=2,
                 GpsTime= (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000,
                 FromChannel="smallchi1"                  
            });
            producerTestService.GpsProducer.ProduceAsync(JT809SubBusinessType.实时上传车辆定位信息.ToValueString(), "粤A23456_2", new GrpcProtos.JT809GpsPosition
            {
                Vno = "粤A23457",
                VColor = 2,
                GpsTime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000,
                FromChannel = "smallchi2"
            });
            producerTestService.GpsProducer.ProduceAsync(JT809SubBusinessType.实时上传车辆定位信息.ToValueString(), "粤A23456_2", new GrpcProtos.JT809GpsPosition
            {
                Vno = "粤A23458",
                VColor = 2,
                GpsTime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000,
                FromChannel = "smallchi3"
            });
            producerTestService.GpsProducer.ProduceAsync(JT809SubBusinessType.实时上传车辆定位信息.ToValueString(), "粤A23456_2", new GrpcProtos.JT809GpsPosition
            {
                Vno = "粤A23459",
                VColor = 2,
                GpsTime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000,
                FromChannel = "smallchi4"
            });
        }

        [Fact]
        public void Test2()
        {
            ProducerTestPartitionService producerTestService = ServiceProvider.GetRequiredService<ProducerTestPartitionService>();          
            producerTestService.SameProducer.ProduceAsync(JT809SubBusinessType.None.ToValueString(), "粤A23452_2", new byte[] { 0x01, 0x02, 0x03 });
            producerTestService.SameProducer.ProduceAsync(JT809SubBusinessType.None.ToValueString(), "粤A23453_2", new byte[] { 0x02, 0x03, 0x04 });
            producerTestService.SameProducer.ProduceAsync(JT809SubBusinessType.None.ToValueString(), "粤A23455_2", new byte[] { 0x03, 0x04, 0x05 });
            producerTestService.SameProducer.ProduceAsync(JT809SubBusinessType.None.ToValueString(), "粤A23452_2", new byte[] { 0x04, 0x05, 0x06 });
        }
    }
}
