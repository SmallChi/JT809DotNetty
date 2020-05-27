using Confluent.Kafka;
using Google.Protobuf;
using JT809.GrpcProtos;
using JT809.KafkaService.Configs;
using JT809.PubSub.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JT809.KafkaService
{
    public sealed class JT809_GpsPosition_Consumer : IJT809Consumer
    {
        private bool _disposed = false;
        public CancellationTokenSource Cts { get; } = new CancellationTokenSource();
        private ILogger logger { get; }
        public string TopicName { get; }

        private IConsumer<string, byte[]> Consumer;
        private readonly JT809ConsumerConfig JT809ConsumerConfig;

        public JT809_GpsPosition_Consumer(
            IOptions<JT809ConsumerConfig> consumerConfigAccessor,
            ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger("JT809_GpsPosition_Consumer");
            JT809ConsumerConfig = consumerConfigAccessor.Value;
            TopicName = JT809ConsumerConfig.TopicName;
            ConsumerBuilder<string, byte[]> consumerBuilder = new ConsumerBuilder<string, byte[]>(consumerConfigAccessor.Value);
            consumerBuilder.SetErrorHandler((consumer, error) =>
            {
                logger.LogError(error.Reason);
            });
            Consumer = consumerBuilder.Build();
        }

        public void OnMessage(Action<(string MsgId, byte[] Data)> callback)
        {
            Task.Run(() =>
            {
                while (!Cts.IsCancellationRequested)
                {
                    try
                    {
                        //如果不指定分区，根据kafka的机制会从多个分区中拉取数据
                        //如果指定分区，根据kafka的机制会从相应的分区中拉取数据
                        //consumers[n].Assign(topicPartitionList[n]);
                        var data = Consumer.Consume(Cts.Token);
                        if (logger.IsEnabled(LogLevel.Debug))
                        {
                            logger.LogDebug($"Topic: {data.Topic} Key: {data.Message.Key} Partition: {data.Partition} Offset: {data.Offset} Data:{string.Join("", data.Message.Value)} TopicPartitionOffset:{data.TopicPartitionOffset}");
                        }
                        callback((data.Message.Key, data.Message.Value));
                    }
                    catch (ConsumeException ex)
                    {
                        logger.LogError(ex, JT809ConsumerConfig.TopicName);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, JT809ConsumerConfig.TopicName);
                    }
                }
            }, Cts.Token);
        }

        public void Subscribe()
        {
            if (_disposed) return;
            //仅有一个分区才需要订阅
            Consumer.Subscribe(JT809ConsumerConfig.TopicName);
        }

        public void Unsubscribe()
        {
            if (_disposed) return;
            Consumer.Unsubscribe();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(true);

        }

        ~JT809_GpsPosition_Consumer()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                Cts.Cancel();
                Consumer.Close();
                Consumer.Dispose();
                Cts.Dispose();
            }
            _disposed = true;
        }
    }
}
