using Confluent.Kafka;
using JT809.PubSub.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JT809.KafkaService
{
    public abstract class JT809PartitionConsumer<T> : JT809ConsumerBase<T>
    {
        private bool _disposed = false;
        public override CancellationTokenSource Cts => new CancellationTokenSource();

        protected ILogger logger { get; }

        private List<TopicPartition> topicPartitionList;

        private JT809PartitionOptions partitionOptions;

        protected override IList<IConsumer<string, T>> Consumers { get; }

        protected JT809PartitionConsumer(
            IOptions<ConsumerConfig> consumerConfigAccessor,
            IOptions<JT809PartitionOptions> partitionOptionsAccessor,
            IOptions<JT809TopicOptions> topicOptionsAccessor,
            ILoggerFactory loggerFactory) : base(topicOptionsAccessor.Value.TopicName, consumerConfigAccessor.Value)
        {
            logger = loggerFactory.CreateLogger("JT809PartitionConsumer");
            partitionOptions = partitionOptionsAccessor.Value;
            topicPartitionList = CreateTopicPartition();
            Consumers = CreateConsumers();
        }

        protected virtual IList<IConsumer<string, T>> CreateConsumers()
        {
            List<IConsumer<string, T>> consumers = new List<IConsumer<string, T>>();
            foreach (var topicPartition in topicPartitionList)
            {
                ConsumerBuilder<string, T> consumerBuilder = new ConsumerBuilder<string, T>(ConsumerConfig);
                consumerBuilder.SetErrorHandler((consumer, error) =>
                {
                    logger.LogError(error.Reason);
                });
                if (Deserializer != null)
                {
                    consumerBuilder.SetValueDeserializer(Deserializer);
                }
                consumers.Add(consumerBuilder.Build());
            }
            return consumers;
        }

        protected virtual List<TopicPartition> CreateTopicPartition()
        {
            var topicPartitions = new List<TopicPartition>();
            if (partitionOptions.AssignPartitions != null && partitionOptions.AssignPartitions.Count > 0)
            {
                foreach (var p in partitionOptions.AssignPartitions)
                {
                    topicPartitions.Add(new TopicPartition(TopicName, new Partition(p)));
                }
            }
            else
            {
                for (int i = 0; i < partitionOptions.Partition; i++)
                {
                    topicPartitions.Add(new TopicPartition(TopicName, new Partition(i)));
                }
            }
            return topicPartitions;
        }

        public override void OnMessage(Action<(string MsgId, T Data)> callback)
        {
            if(logger.IsEnabled( LogLevel.Debug))
                logger.LogDebug($"consumers:{Consumers.Count},topicPartitionList:{topicPartitionList.Count}");
            for (int i = 0; i < Consumers.Count; i++)
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
                            Consumers[n].Assign(topicPartitionList[n]);
                            var data = Consumers[n].Consume(Cts.Token);
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

        public override void Subscribe()
        {
            if (_disposed) return;
        }

        public override void Unsubscribe()
        {
            if (_disposed) return;
            foreach (var c in Consumers)
            {
                c.Unsubscribe();
            }
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(true);

        }

        ~JT809PartitionConsumer()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                Cts.Cancel();
                foreach (var c in Consumers)
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
