using Confluent.Kafka;
using JT809.PubSub.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JT809.KafkaService
{
    public abstract class JT809Consumer<T> : IJT808ConsumerOfT<T>
    {
        private bool _disposed = false;
        public CancellationTokenSource Cts => new CancellationTokenSource();

        public virtual string TopicName => JT809Constants.JT809TopicName;

        private readonly ILogger logger;

        private List<TopicPartition> topicPartitionList;

        private IList<IConsumer<string, T>> consumers;

        protected abstract JT809PartitionOptions PartitionOptions { get; }

        protected virtual Deserializer<T> Deserializer { get; }

        protected JT809Consumer(
            ConsumerConfig consumerConfig,
            ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger("JT809Consumer");
            CreateTopicPartition();
            consumers = new List<IConsumer<string, T>>();
            foreach(var topicPartition in topicPartitionList)
            {
                ConsumerBuilder<string, T> consumerBuilder = new ConsumerBuilder<string, T>(consumerConfig);
                consumerBuilder.SetErrorHandler((consumer, error) =>
                {
                    logger.LogError(error.Reason);
                });
                if (Deserializer != null)
                {
                    consumerBuilder.SetValueDeserializer(Deserializer);
                }
                if (PartitionOptions.Partition > 1)
                {
                    consumerBuilder.SetPartitionsAssignedHandler((c, p) => {
                        p.Add(topicPartition);
                    });
                }
                consumers.Add(consumerBuilder.Build());
            }
        }

        private void CreateTopicPartition()
        {
            topicPartitionList = new List<TopicPartition>();
            if (PartitionOptions.Partition > 1)
            {
                if(PartitionOptions.AssignPartitions!=null && PartitionOptions.AssignPartitions.Count>0)
                {
                    foreach(var p in PartitionOptions.AssignPartitions)
                    {
                        topicPartitionList.Add(new TopicPartition(TopicName, new Partition(p)));
                    }                   
                }
                else
                {
                    for (int i = 0; i < PartitionOptions.Partition; i++)
                    {
                        topicPartitionList.Add(new TopicPartition(TopicName, new Partition(i)));
                    }
                }
            }
            else
            {
                for (int i = 0; i < PartitionOptions.Partition; i++)
                {
                    topicPartitionList.Add(new TopicPartition(TopicName, new Partition(i)));
                }
            }
        }

        public void OnMessage(Action<(string MsgId, T data)> callback)
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
                            //consumers[n].Assign(topicPartitionList[n]);
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
            if (_disposed) return;
            //仅有一个分区才需要订阅
            if (topicPartitionList.Count == 1)
            {
                consumers[0].Subscribe(TopicName);
            }
        }

        public void Unsubscribe()
        {
            if (_disposed) return;
            foreach(var c in consumers)
            {
                c.Unsubscribe();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(true);

        }

        ~JT809Consumer()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                Cts.Cancel();
                foreach (var c in consumers)
                {
                    c.Close();
                    c.Dispose();
                }
                Cts.Dispose();
            }
            _disposed = true;
        }
    }
}
