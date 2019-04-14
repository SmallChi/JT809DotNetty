using Confluent.Kafka;
using JT809.GrpcProtos;
using JT809.KafkaService.Partitions;
using JT809.PubSub.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.KafkaService
{
    public static  class JT809KafkaServiceExtensions
    {
        public static IServiceCollection AddJT809KafkaProducerService(this IServiceCollection serviceDescriptors, IConfiguration configuration)
        {
            serviceDescriptors.Configure<ProducerConfig>(configuration.GetSection("KafkaProducerConfig"));
            serviceDescriptors.AddSingleton(typeof(JT809Producer<byte[]>), (service) => {
                var producerConfig = service.GetRequiredService<IOptions<ProducerConfig>>();
                return new JT809_Same_Producer(new JT809TopicOptions { TopicName = "jt809.same" }, producerConfig);
            });
            serviceDescriptors.AddSingleton(typeof(JT809Producer<JT809GpsPosition>), (service) => {
                var producerConfig = service.GetRequiredService<IOptions<ProducerConfig>>();
                return new JT809_GpsPositio_Producer(new JT809TopicOptions { TopicName = "jt809.gps" }, producerConfig);
            });
            return serviceDescriptors;
        }

        public static IServiceCollection AddJT809KafkaProducerPartitionsService(this IServiceCollection serviceDescriptors, IConfiguration configuration, Action<JT809PartitionOptions> action)
        {
            serviceDescriptors.Configure<ProducerConfig>(configuration.GetSection("KafkaProducerConfig"));
            serviceDescriptors.Configure(action);       
            serviceDescriptors.AddSingleton<IJT809ProducerPartitionFactory, JT809GpsPositionProducerPartitionFactoryImpl>();
            serviceDescriptors.AddSingleton(typeof(JT809PartitionProducer<byte[]>), (service) => {
                var producerConfig = service.GetRequiredService<IOptions<ProducerConfig>>();
                var producerPartitionFactory = service.GetRequiredService<IJT809ProducerPartitionFactory>();
                var partitionOptions = service.GetRequiredService<IOptions<JT809PartitionOptions>>();
                return new JT809_Same_Partition_Producer(new JT809TopicOptions { TopicName = "jt809.partition.same" }, producerConfig, producerPartitionFactory, partitionOptions);
            });
            serviceDescriptors.AddSingleton(typeof(JT809PartitionProducer<JT809GpsPosition>), (service) => {
                var producerConfig = service.GetRequiredService<IOptions<ProducerConfig>>();
                var producerPartitionFactory = service.GetRequiredService<IJT809ProducerPartitionFactory>();
                var partitionOptions = service.GetRequiredService<IOptions<JT809PartitionOptions>>();
                return new JT809_GpsPositio_Partition_Producer(new JT809TopicOptions { TopicName = "jt809.partition.gps" }, producerConfig, producerPartitionFactory, partitionOptions);
            });
            return serviceDescriptors;
        }

        public static IServiceCollection AddJT809KafkaConsumerService(this IServiceCollection serviceDescriptors, IConfiguration configuration)
        {
            serviceDescriptors.Configure<ConsumerConfig>(configuration.GetSection("KafkaConsumerConfig"));
            serviceDescriptors.AddSingleton(typeof(JT809Consumer<byte[]>), (service)=> {
                var loggerFactory = service.GetRequiredService<ILoggerFactory>();
                var consumerConfig = service.GetRequiredService<IOptions<ConsumerConfig>>();
                consumerConfig.Value.GroupId = "JT809.same.Test";
                return new JT809_Same_Consumer(new JT809TopicOptions { TopicName = "jt809.same" }, consumerConfig, loggerFactory);
            });
            serviceDescriptors.AddSingleton(typeof(JT809Consumer<JT809GpsPosition>), (service) => {
                var loggerFactory = service.GetRequiredService<ILoggerFactory>();
                var consumerConfig = service.GetRequiredService<IOptions<ConsumerConfig>>();
                consumerConfig.Value.GroupId = "JT809.gps.Test";
                return new JT809_GpsPosition_Consumer(new JT809TopicOptions { TopicName = "jt809.gps" }, consumerConfig, loggerFactory);
            });
            return serviceDescriptors;
        }

        public static IServiceCollection AddJT809KafkaConsumerPartitionsService(this IServiceCollection serviceDescriptors, IConfiguration configuration, Action<JT809PartitionOptions> action)
        {
            serviceDescriptors.Configure<ConsumerConfig>(configuration.GetSection("KafkaConsumerConfig"));
            serviceDescriptors.Configure(action); 
            serviceDescriptors.AddSingleton(typeof(JT809PartitionConsumer<byte[]>), (service) => {
                var loggerFactory = service.GetRequiredService<ILoggerFactory>();
                var consumerConfig = service.GetRequiredService<IOptions<ConsumerConfig>>();
                var partitionOptions = service.GetRequiredService<IOptions<JT809PartitionOptions>>();
                consumerConfig.Value.GroupId = "JT809.partition.same.Test";
                return new JT809_Same_Partition_Consumer(consumerConfig, partitionOptions,new JT809TopicOptions { TopicName = "jt809.partition.same" } , loggerFactory);
            });
            serviceDescriptors.AddSingleton(typeof(JT809PartitionConsumer<JT809GpsPosition>), (service) => {
                var loggerFactory = service.GetRequiredService<ILoggerFactory>();
                var consumerConfig = service.GetRequiredService<IOptions<ConsumerConfig>>();
                var partitionOptions = service.GetRequiredService<IOptions<JT809PartitionOptions>>();
                consumerConfig.Value.GroupId = "JT809.partition.gps.Test";
                return new JT809_GpsPosition_Partition_Consumer(consumerConfig, partitionOptions,new JT809TopicOptions { TopicName = "jt809.partition.gps" }, loggerFactory);
            });
            return serviceDescriptors;
        }
    }
}
