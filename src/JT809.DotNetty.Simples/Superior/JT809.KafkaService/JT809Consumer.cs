using Confluent.Kafka;
using JT809.KafkaService.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JT809.KafkaService
{
    public abstract class JT809Consumer<T> : JT809ConsumerBase<T>
    {
        private bool _disposed = false;
        public override CancellationTokenSource Cts { get; }= new CancellationTokenSource();

        protected ILogger logger { get; }

        protected override IConsumer<string, T> Consumer { get; }

        protected JT809Consumer(
            IOptions<JT809ConsumerConfig> consumerConfigAccessor,
            ILoggerFactory loggerFactory) 
            : base(consumerConfigAccessor.Value)
        {
            logger = loggerFactory.CreateLogger("JT809Consumer");
            ConsumerBuilder<string, T> consumerBuilder = new ConsumerBuilder<string, T>(ConsumerConfig);
            consumerBuilder.SetErrorHandler((consumer, error) =>
            {
                logger.LogError(error.Reason);
            });
            Consumer = consumerBuilder.Build();
        }

        public override void OnMessage(Action<(string MsgId, T Data)> callback)
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
                        logger.LogError(ex, ConsumerConfig.TopicName);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, ConsumerConfig.TopicName);
                    }
                }
            }, Cts.Token);            
        }

        public override void Subscribe()
        {
            if (_disposed) return;
            //仅有一个分区才需要订阅
            Consumer.Subscribe(ConsumerConfig.TopicName);
        }

        public override void Unsubscribe()
        {
            if (_disposed) return;
            Consumer.Unsubscribe(); 
        }

        public override void Dispose()
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
                Consumer.Close();
                Consumer.Dispose();
                Cts.Dispose();
            }
            _disposed = true;
        }
    }
}
