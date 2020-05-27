using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using JT809.Protocol.Enums;
using JT809.Protocol.Extensions;
using System.Threading;
using Google.Protobuf;
using JT809.GrpcProtos;

namespace JT809.KafkaServiceTest
{
    public  class ConsumerTest: TestConsumerBase
    {
        [Fact]
        public void Test1()
        {
            ConsumerTestService consumerTestService = ServiceProvider.GetRequiredService<ConsumerTestService>();
            consumerTestService.GpsConsumer.OnMessage((Message)=> 
            {
                Assert.Equal(JT809SubBusinessType.实时上传车辆定位信息.ToValueString(), Message.MsgId);
                JT809GpsPosition jT809GpsPosition = JT809GpsPosition.Parser.ParseFrom(Message.Data);
                Assert.Equal("粤A23456", jT809GpsPosition.Vno);
                Assert.Equal(2, jT809GpsPosition.VColor);
                Assert.Equal("smallchi", jT809GpsPosition.FromChannel);
            });
            consumerTestService.GpsConsumer.Subscribe();

            Thread.Sleep(100000);
        }

        [Fact]
        public void Test2()
        {
            ConsumerTestService consumerTestService = ServiceProvider.GetRequiredService<ConsumerTestService>();
            consumerTestService.SameConsumer.OnMessage((Message) =>
            {
                Assert.Equal(JT809SubBusinessType.None.ToValueString(), Message.MsgId);
                Assert.Equal(new byte[] { 0x01, 0x02, 0x03 }, Message.Data);
            });
            consumerTestService.SameConsumer.Subscribe();

            Thread.Sleep(100000);
        }
    }
}
