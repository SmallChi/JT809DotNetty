using Confluent.Kafka;
using Google.Protobuf;
using JT809.GrpcProtos;
using JT809.PubSub.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JT809.KafkaService
{
    public class JT809_GpsPosition_Consumer : IJT808ConsumerOfT<JT809GpsPosition>
    {
        public CancellationTokenSource Cts => new CancellationTokenSource();

        public string TopicName => JT809Constants.JT809TopicName;

        private readonly ILogger<JT809_GpsPosition_Consumer> logger;

        private readonly List<TopicPartition> topicPartitionList;

        private readonly List<IConsumer<string, JT809GpsPosition>> consumers;

        private readonly JT809PartitionOptions partition;

        public JT809_GpsPosition_Consumer(
            IOptions<JT809PartitionOptions> partitionAccessor,
            IOptions<ConsumerConfig> consumerConfigAccessor, 
            ILoggerFactory loggerFactory)
        {
            partition = partitionAccessor.Value;
            logger = loggerFactory.CreateLogger<JT809_GpsPosition_Consumer>();
            topicPartitionList = new List<TopicPartition>();
            consumers = new List<IConsumer<string, JT809GpsPosition>>();
            for (int i=0;i< partition.Partition; i++)
            {
                topicPartitionList.Add(new TopicPartition(TopicName, new Partition(i)));
                consumers.Add(new ConsumerBuilder<string, JT809GpsPosition>(consumerConfigAccessor.Value)
                        .SetErrorHandler((consumer, error) => {
                            logger.LogError(error.Reason);
                        })
                        .SetValueDeserializer((data, isNull) => {
                            if (isNull) return default;
                            return new MessageParser<JT809GpsPosition>(() => new JT809GpsPosition())
                                   .ParseFrom(data.ToArray());
                        })
                        .Build());
            }
        }

        public JT809_GpsPosition_Consumer(
            IOptions<ConsumerConfig> consumerConfigAccessor,
            ILoggerFactory loggerFactory):this(new JT809PartitionOptions (), consumerConfigAccessor, loggerFactory)
        {

        }

        public void OnMessage(Action<(string MsgId, JT809GpsPosition data)> callback)
        {
            logger.LogDebug($"consumers:{consumers.Count},topicPartitionList:{topicPartitionList.Count}");
            for (int i = 0; i < consumers.Count; i++)
            {
                Task.Factory.StartNew((num) =>
                {
                    int n = (int)num;
                    while (!Cts.IsCancellationRequested)
                    {
                        try
                        {
                            //如果不指定分区，根据kafka的机制会从多个分区中拉取数据
                            //如果指定分区，根据kafka的机制会从相应的分区中拉取数据
                            consumers[n].Assign(topicPartitionList[n]);
                            var data = consumers[n].Consume(Cts.Token);
                            if (logger.IsEnabled(LogLevel.Debug))
                            {
                                logger.LogDebug($"Topic: {data.Topic} Key: {data.Key} Partition: {data.Partition} Offset: {data.Offset} Data:{string.Join("", data.Value)} TopicPartitionOffset:{data.TopicPartitionOffset}");
                            }
                            callback((data.Key, data.Value));
                        }
                        catch (ConsumeException ex)
                        {
                            logger.LogError(ex, TopicName);
                            Thread.Sleep(1000);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, TopicName);
                            Thread.Sleep(1000);
                        }
                    }
                }, i, Cts.Token);
            }
        }

        public void Subscribe()
        {
            //仅有一个分区才需要订阅
            if (topicPartitionList.Count == 1)
            {
                consumers[0].Subscribe(TopicName);
            }
        }

        public void Unsubscribe()
        {
            consumers.ForEach(consumer => consumer.Unsubscribe());
        }

        public void Dispose()
        {
            Cts.Cancel();
            consumers.ForEach(consumer => {
                consumer.Close();
                consumer.Dispose();
            });
        }
    }
}
