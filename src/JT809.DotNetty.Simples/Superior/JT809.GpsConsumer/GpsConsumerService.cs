using JT809.GrpcProtos;
using JT809.KafkaService;
using JT809.PubSub.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JT809.GpsConsumer
{
    public class GpsConsumerService : IHostedService
    {
        private readonly JT809_GpsPosition_Consumer jT808Consumer;

        private readonly ILogger logger;

        public GpsConsumerService(
            ILoggerFactory loggerFactory,
            JT809_GpsPosition_Consumer jT808Consumer)
        {
            this.jT808Consumer = jT808Consumer;
            logger = loggerFactory.CreateLogger<GpsConsumerService>();
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogDebug("StartAsync ...");
            jT808Consumer.OnMessage((Message) =>
            {
                //处理数据来源FromChannel
                //处理入库
                //处理缓存
                //...
                logger.LogDebug($"Receive MsgId:{Message.MsgId}");
                logger.LogDebug($"Receive Data:{JsonConvert.SerializeObject(Message.Data)}");
            });
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            jT808Consumer.Unsubscribe();
            logger.LogDebug("StopAsync ...");
            return Task.CompletedTask;
        }
    }
}
