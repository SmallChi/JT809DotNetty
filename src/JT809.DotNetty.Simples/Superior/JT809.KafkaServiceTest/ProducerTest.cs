using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using JT809.Protocol.Enums;
using JT809.Protocol.Extensions;

namespace JT809.KafkaServiceTest
{
    public class ProducerTest: TestProducerBase
    {
        [Fact]
        public void Test1()
        {
            ProducerTestService producerTestService = ServiceProvider.GetRequiredService<ProducerTestService>();
            producerTestService.GpsProducer.ProduceAsync(JT809SubBusinessType.实时上传车辆定位信息.ToValueString(), "粤A23456_2", new GrpcProtos.JT809GpsPosition
            {
                 Vno= "粤A23456",
                 VColor=2,
                 GpsTime= (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000,
                 FromChannel="smallchi"                  
            });
        }

        [Fact]
        public void Test2()
        {
            ProducerTestService producerTestService = ServiceProvider.GetRequiredService<ProducerTestService>();          
            producerTestService.SameProducer.ProduceAsync(JT809SubBusinessType.None.ToValueString(), "粤A23457_2", new byte[] { 0x01, 0x02, 0x03 });
        }
    }
}
