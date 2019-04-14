using Confluent.Kafka;
using Confluent.Kafka.Admin;
using JT809.PubSub.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace JT809.KafkaService
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class JT809PartitionProducer<T> : JT809ProducerBase<T>
    {
        private bool _disposed = false;

        private ConcurrentDictionary<string, TopicPartition> TopicPartitionCache;

        private readonly IJT809ProducerPartitionFactory ProducerPartitionFactory;

        private readonly JT809PartitionOptions PartitionOptions;

        protected override IProducer<string, T> Producer { get; }

        protected virtual IProducer<string, T> CreateProducer()
        {
            ProducerBuilder<string, T> producerBuilder = new ProducerBuilder<string, T>(ProducerConfig);
            if (Serializer != null)
            {
                producerBuilder.SetValueSerializer(Serializer);
            }
            return  producerBuilder.Build();
        }

        protected JT809PartitionProducer(
            IOptions<JT809TopicOptions> topicOptionAccessor,
            IOptions<ProducerConfig> producerConfig,
            IJT809ProducerPartitionFactory producerPartitionFactory,
            IOptions<JT809PartitionOptions> partitionOptionsAccessor)
           : base(topicOptionAccessor.Value.TopicName, producerConfig.Value)
        {
            PartitionOptions = partitionOptionsAccessor.Value;
            ProducerPartitionFactory = producerPartitionFactory;
            Producer = CreateProducer();
            CreatePartition();
        }

        private void CreatePartition()
        {
            if (PartitionOptions != null)
            {
                TopicPartitionCache = new ConcurrentDictionary<string, TopicPartition>();
                if (PartitionOptions.Partition > 1)
                {
                    using (var adminClient = new AdminClient(Producer.Handle))
                    {
                        try
                        {
                            adminClient.CreateTopicsAsync(new TopicSpecification[] { new TopicSpecification { Name = TopicName, NumPartitions = 1, ReplicationFactor = 1 } }).Wait();
                        }
                        catch (AggregateException ex)
                        {
                            //{Confluent.Kafka.Admin.CreateTopicsException: An error occurred creating topics: [jt809]: [Topic 'jt809' already exists.].}
                            if (ex.InnerException is Confluent.Kafka.Admin.CreateTopicsException exception)
                            {

                            }
                            else
                            {
                                //记录日志
                                //throw ex.InnerException;
                            }
                        }
                        try
                        {
                            //topic IncreaseTo 只增不减
                            adminClient.CreatePartitionsAsync(
                                            new List<PartitionsSpecification>
                                            {
                                                new PartitionsSpecification
                                                {
                                                        IncreaseTo = PartitionOptions.Partition,
                                                        Topic=TopicName
                                                }
                                            }
                                        ).Wait();
                        }
                        catch (AggregateException ex)
                        {
                            //记录日志
                            // throw ex.InnerException;
                        }
                    }
                }
            }
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                Producer.Dispose();
            }
            _disposed = true;
        }

        public override async void ProduceAsync(string msgId, string vno_color, T data)
        {
            if (_disposed) return;
            if (PartitionOptions != null)
            {
                if (PartitionOptions.Partition > 1)
                {
                    if (!TopicPartitionCache.TryGetValue(vno_color, out TopicPartition topicPartition))
                    {
                        topicPartition = new TopicPartition(TopicName, new Partition(ProducerPartitionFactory.CreatePartition(TopicName, msgId, vno_color)));
                        TopicPartitionCache.TryAdd(vno_color, topicPartition);
                    }
                   await  Producer.ProduceAsync(topicPartition, new Message<string, T>
                    {
                        Key = msgId,
                        Value = data
                    });
                }
                else
                {
                    await Producer.ProduceAsync(TopicName, new Message<string, T>
                    {
                        Key = msgId,
                        Value = data
                    });
                }
            }
            else
            {
                await Producer.ProduceAsync(TopicName, new Message<string, T>
                {
                    Key = msgId,
                    Value = data
                });
            }
        }

        ~JT809PartitionProducer()
        {
            Dispose(false);
        }
    }
}
