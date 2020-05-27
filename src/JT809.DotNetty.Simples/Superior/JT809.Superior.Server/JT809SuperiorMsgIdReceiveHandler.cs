using Google.Protobuf;
using JT809.DotNetty.Core.Handlers;
using JT809.DotNetty.Core.Interfaces;
using JT809.DotNetty.Core.Metadata;
using JT809.GrpcProtos;
using JT809.KafkaService;
using JT809.Protocol;
using JT809.Protocol.SubMessageBody;
using JT809.PubSub.Abstractions;
using JT809.Superior.Server.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.Superior.Server
{
    public sealed class JT809SuperiorMsgIdReceiveHandler : JT809SuperiorMsgIdReceiveHandlerBase
    {
        private readonly JT809_GpsPositio_Producer producer;
        private readonly JT809GpsOptions gpsOptions;
        public JT809SuperiorMsgIdReceiveHandler(
            IOptions<JT809GpsOptions>jt809GpsAccessor,
            JT809_GpsPositio_Producer producer,
            ILoggerFactory loggerFactory, 
            IJT809SubordinateLoginService jT809SubordinateLoginService, 
            IJT809VerifyCodeGenerator verifyCodeGenerator) 
            : base(loggerFactory, jT809SubordinateLoginService, verifyCodeGenerator)
        {
            this.producer = producer;
            this.gpsOptions = jt809GpsAccessor.Value;
        }

        public override JT809Response Msg0x1200_0x1202(JT809Request request)
        {
            var exchangeMessageBodies = request.Package.Bodies as JT809ExchangeMessageBodies;
            var gpsBodies = exchangeMessageBodies.SubBodies as JT809_0x1200_0x1202;
            JT809GpsPosition gpsPosition = new JT809GpsPosition();
            gpsPosition.Vno = exchangeMessageBodies.VehicleNo;
            gpsPosition.VColor = (byte)exchangeMessageBodies.VehicleColor;
            gpsPosition.Alarm = (int)gpsBodies.VehiclePosition.Alarm;
            gpsPosition.Altitude = gpsBodies.VehiclePosition.Altitude;
            gpsPosition.Direction = gpsBodies.VehiclePosition.Direction;
            gpsPosition.Encrypt = (byte)gpsBodies.VehiclePosition.Encrypt;
            gpsPosition.State = (int)gpsBodies.VehiclePosition.State;
            gpsPosition.Lat = gpsBodies.VehiclePosition.Lat;
            gpsPosition.Lon= gpsBodies.VehiclePosition.Lon;
            gpsPosition.Vec1 = gpsBodies.VehiclePosition.Vec1;
            gpsPosition.Vec2 = gpsBodies.VehiclePosition.Vec2;
            gpsPosition.Vec3 =(int)gpsBodies.VehiclePosition.Vec3;
            gpsPosition.GpsTime = (new DateTime(
                gpsBodies.VehiclePosition.Year,
                gpsBodies.VehiclePosition.Month,
                gpsBodies.VehiclePosition.Day,
                gpsBodies.VehiclePosition.Hour,
                gpsBodies.VehiclePosition.Minute,
                gpsBodies.VehiclePosition.Second).ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            gpsPosition.FromChannel = gpsOptions.FromChannel;
            producer.ProduceAsync($"{0x1202}", $"{exchangeMessageBodies.VehicleNo}{exchangeMessageBodies.VehicleColor}", gpsPosition.ToByteArray());
            return base.Msg0x1200_0x1202(request);
        }
    }
}
