using JT809.DotNetty.Core.Clients;
using JT809.DotNetty.Core.Metadata;
using JT809.Protocol.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JT809.Protocol.SubMessageBody;
using JT809.Protocol.Metadata;
using JT809.Protocol.MessageBody;
using JT809.Protocol.Enums;

namespace JT809.Inferior.Client
{
    public class JT809InferiorService : IHostedService
    {
        private readonly JT809MainClient mainClient;
        private readonly ILogger<JT809InferiorService> logger;
        public JT809InferiorService(
            ILoggerFactory loggerFactory,
            JT809MainClient mainClient)
        {
            this.mainClient = mainClient;
            logger = loggerFactory.CreateLogger<JT809InferiorService>();
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            //5B0000001F0000053B100201341725010000000000270F00000004E8A6F25D
            var connect = mainClient.Login("127.0.0.1", 809, new JT809_0x1001
            {
                DownLinkIP = "127.0.0.1",
                DownLinkPort = 1809,
                UserId = 123456,
                Password = "12345678"
            }).Result;
            if (connect)
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        JT809.Protocol.MessageBody.JT809_0x1200 jT809_0X1200 = new Protocol.MessageBody.JT809_0x1200();
                        jT809_0X1200.VehicleColor = Protocol.Enums.JT809VehicleColorType.黄色;
                        jT809_0X1200.VehicleNo = "粤A12345";
                        jT809_0X1200.SubBusinessType = Protocol.Enums.JT809SubBusinessType.实时上传车辆定位信息.ToUInt16Value();
                        jT809_0X1200.SubBodies = new JT809_0x1200_0x1202()
                        {
                            VehiclePosition = new JT809VehiclePositionProperties
                            {
                                Day = (byte)(DateTime.Now.Day),
                                Month = (byte)(DateTime.Now.Month),
                                Year = (ushort)(DateTime.Now.Year),
                                Hour = (byte)(DateTime.Now.Hour),
                                Minute = (byte)(DateTime.Now.Minute),
                                Second = (byte)(DateTime.Now.Second),
                                Alarm = 1,
                                Direction = 2,
                                State = 2,
                                Altitude = 32,
                                Lat = 122334565,
                                Lon = 12354563,
                                Vec1 = 112,
                                Vec2 = 22,
                                Vec3 = 12
                            }
                        };
                        var package = JT809.Protocol.Enums.JT809BusinessType.主链路车辆动态信息交换业务.Create(jT809_0X1200);
                        mainClient.SendAsync(new JT809Response(package, 256));
                        logger.LogDebug($"Thread:{Thread.CurrentThread.ManagedThreadId}-2s");
                        Thread.Sleep(2000);
                    }
                });
                Task.Run(() =>
                {
                    while (true)
                    {
                        JT809.Protocol.MessageBody.JT809_0x1200 jT809_0X1200 = new Protocol.MessageBody.JT809_0x1200();
                        jT809_0X1200.VehicleColor = Protocol.Enums.JT809VehicleColorType.黄色;
                        jT809_0X1200.VehicleNo = "粤A12346";
                        jT809_0X1200.SubBusinessType = Protocol.Enums.JT809SubBusinessType.实时上传车辆定位信息.ToUInt16Value();
                        jT809_0X1200.SubBodies = new JT809_0x1200_0x1202()
                        {
                            VehiclePosition = new JT809VehiclePositionProperties
                            {
                                Day = (byte)(DateTime.Now.Day),
                                Month = (byte)(DateTime.Now.Month),
                                Year = (byte)(DateTime.Now.Year),
                                Hour = (byte)(DateTime.Now.Hour),
                                Minute = (byte)(DateTime.Now.Minute),
                                Second = (byte)(DateTime.Now.Second),
                                Alarm = 1,
                                Direction = 2,
                                State = 2,
                                Altitude = 32,
                                Lat = 122334565,
                                Lon = 12354563,
                                Vec1 = 112,
                                Vec2 = 22,
                                Vec3 = 12
                            }
                        };
                        var package = JT809BusinessType.主链路车辆动态信息交换业务.Create(jT809_0X1200);
                        mainClient.SendAsync(new JT809Response(package, 256));
                        logger.LogDebug($"Thread:{Thread.CurrentThread.ManagedThreadId}-4s");
                        Thread.Sleep(4000);
                    }
                });
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
