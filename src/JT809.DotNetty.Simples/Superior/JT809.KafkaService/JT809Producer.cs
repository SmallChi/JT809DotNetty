using Confluent.Kafka;
using Confluent.Kafka.Admin;
using JT809.PubSub.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace JT809.KafkaService
{
    public abstract class JT809Producer<T> : IJT809ProducerOfT<T>
    {
        private bool _disposed = false;

        public virtual string TopicName => JT809Constants.JT809TopicName;

        private ConcurrentDictionary<string, TopicPartition> TopicPartitionCache;

        private IProducer<string, T> producer;

        protected virtual IJT809ProducerPartitionFactory ProducerPartitionFactory { get; }

        protected virtual JT809PartitionOptions PartitionOptions { get; }

        protected abstract ProducerConfig ProducerConfig { get; }

        protected JT809Producer()
        {
            CreateProducer();
            if (PartitionOptions != null)
            {
                TopicPartitionCache = new ConcurrentDictionary<string, TopicPartition>();
                if (PartitionOptions.Partition > 1)
                {
                    using (var adminClient = new AdminClient(producer.Handle))
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

        protected abstract IProducer<string, T> CreateProducer();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(true);

        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                producer.Dispose();
            }
            _disposed = true;
        }

        public void ProduceAsync(string msgId, string vno_color, T data)
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
                    producer.ProduceAsync(topicPartition, new Message<string, T>
                    {
                        Key = msgId,
                        Value = data
                    });
                }
                else
                {
                    producer.ProduceAsync(TopicName, new Message<string, T>
                    {
                        Key = msgId,
                        Value = data
                    });
                }
            }
            else
            {
                producer.ProduceAsync(TopicName, new Message<string, T>
                {
                    Key = msgId,
                    Value = data
                });
            }
        }

        ~JT809Producer()
        {
            Dispose(false);
        }
    }
}
