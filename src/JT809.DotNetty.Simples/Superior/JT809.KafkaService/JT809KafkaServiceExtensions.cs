using Confluent.Kafka;
using JT809.GrpcProtos;
using JT809.KafkaService.Configs;
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
            serviceDescriptors.Configure<JT809ProducerConfig>(configuration.GetSection("JT809ProducerConfig"));
            serviceDescriptors.AddSingleton<JT809_GpsPositio_Producer>();
            return serviceDescriptors;
        }


        public static IServiceCollection AddJT809KafkaConsumerService(this IServiceCollection serviceDescriptors, IConfiguration configuration)
        {
            serviceDescriptors.Configure<JT809ConsumerConfig>(configuration.GetSection("JT809ConsumerConfig"));
            serviceDescriptors.AddSingleton<JT809_GpsPosition_Consumer>();
            return serviceDescriptors;
        }

        public static IServiceCollection AddJT809KafkaSameProducerService(this IServiceCollection serviceDescriptors, IConfiguration configuration)
        {
            serviceDescriptors.Configure<JT809SameProducerConfig>(configuration.GetSection("JT809SameProducerConfig"));
            serviceDescriptors.AddSingleton<JT809_GpsPositio_Producer>();
            return serviceDescriptors;
        }


        public static IServiceCollection AddJT809KafkaSameConsumerService(this IServiceCollection serviceDescriptors, IConfiguration configuration)
        {
            serviceDescriptors.Configure<JT809SameConsumerConfig>(configuration.GetSection("JT809SameConsumerConfig"));
            serviceDescriptors.AddSingleton<JT809_Same_Consumer>();
            return serviceDescriptors;
        }

    }
}
