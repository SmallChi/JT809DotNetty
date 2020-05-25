using Confluent.Kafka;
using JT809.GrpcProtos;
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
            serviceDescriptors.AddSingleton(typeof(IJT809ProducerOfT<byte[]>), (service) => {
                var producerConfig = service.GetRequiredService<IOptions<ProducerConfig>>();
#warning JT809_Same_Producer
                //return new JT809_Same_Producer(new JT809TopicOptions { TopicName = "jt809.same" }, producerConfig);
                return null;
            });
            serviceDescriptors.AddSingleton(typeof(IJT809ProducerOfT<JT809GpsPosition>), (service) => {
                var producerConfig = service.GetRequiredService<IOptions<ProducerConfig>>();
#warning JT809_GpsPositio_Producer
                //return new JT809_GpsPositio_Producer(new JT809TopicOptions { TopicName = "jt809.gps" }, producerConfig);
                return null;
            });
            return serviceDescriptors;
        }


        public static IServiceCollection AddJT809KafkaConsumerService(this IServiceCollection serviceDescriptors, IConfiguration configuration)
        {
            serviceDescriptors.Configure<ConsumerConfig>(configuration.GetSection("KafkaConsumerConfig"));
            serviceDescriptors.AddSingleton(typeof(IJT808ConsumerOfT<byte[]>), (service)=> {
                var loggerFactory = service.GetRequiredService<ILoggerFactory>();
                var consumerConfig = service.GetRequiredService<IOptions<ConsumerConfig>>();
                consumerConfig.Value.GroupId = "JT809.same.Test";
#warning JT809_Same_Consumer
                //return new JT809_Same_Consumer(new JT809TopicOptions { TopicName = "jt809.same" }, consumerConfig, loggerFactory);
                return null;
            });
            serviceDescriptors.AddSingleton(typeof(IJT808ConsumerOfT<JT809GpsPosition>), (service) => {
                var loggerFactory = service.GetRequiredService<ILoggerFactory>();
                var consumerConfig = service.GetRequiredService<IOptions<ConsumerConfig>>();
                consumerConfig.Value.GroupId = "JT809.gps.Test";
#warning JT809_GpsPosition_Consumer
                //return new JT809_GpsPosition_Consumer(new JT809TopicOptions { TopicName = "jt809.gps" }, consumerConfig, loggerFactory);
                return null;
            });
            return serviceDescriptors;
        }
    }
}
