using Confluent.Kafka;
using JT809.GrpcProtos;
using JT809.PubSub.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.KafkaService
{
    /// <summary>
    /// 
    /// https://github.com/aspnet/Extensions/blob/master/src/DependencyInjection/DI.Specification.Tests/src/DependencyInjectionSpecificationTests.cs
    /// https://github.com/aspnet/Extensions/pull/536
    /// </summary>
    public static  class JT809KafkaServiceExtensions
    {
        public static IServiceCollection AddJT809KafkaProducerService(this IServiceCollection serviceDescriptors, IConfiguration configuration)
        {
            serviceDescriptors.Configure<ProducerConfig>(configuration.GetSection("KafkaProducerConfig"));
            serviceDescriptors.AddSingleton(typeof(JT809Producer<byte[]>), typeof(JT809_Same_Producer));
            serviceDescriptors.AddSingleton(typeof(JT809Producer<JT809GpsPosition>), typeof(JT809_GpsPositio_Producer));
            return serviceDescriptors;
        }

        public static IServiceCollection AddJT809KafkaProducerPartitionsService<TPartitionFactory>(this IServiceCollection serviceDescriptors,Action<JT809PartitionOptions> action)
            where TPartitionFactory: IJT809ProducerPartitionFactory
        {
            serviceDescriptors.Configure(action);
            serviceDescriptors.AddSingleton(typeof(IJT809ProducerPartitionFactory), typeof(TPartitionFactory));
            return serviceDescriptors;
        }

        public static IServiceCollection AddJT809KafkaProducerPartitionsService<TPartitionFactory>(this IServiceCollection serviceDescriptors, IConfiguration configuration)
           where TPartitionFactory : IJT809ProducerPartitionFactory
        {
            serviceDescriptors.Configure<JT809PartitionOptions>(configuration.GetSection("JT809PartitionOptions"));
            serviceDescriptors.AddSingleton(typeof(IJT809ProducerPartitionFactory), typeof(TPartitionFactory));
            return serviceDescriptors;
        }

        public static IServiceCollection AddJT809KafkaConsumerService(this IServiceCollection serviceDescriptors, IConfiguration configuration, Action<JT809PartitionOptions> action = null)
        {
            serviceDescriptors.Configure<ConsumerConfig>(configuration.GetSection("KafkaConsumerConfig"));
            if (configuration.GetSection("JT809PartitionOptions").Exists())
            {
                serviceDescriptors.Configure<JT809PartitionOptions>(configuration.GetSection("JT809PartitionOptions"));
            }
            if (action != null)
            {
                serviceDescriptors.Configure<JT809PartitionOptions>(action);
            }
            serviceDescriptors.AddSingleton(typeof(JT809Consumer<byte[]>), typeof(JT809_Same_Consumer));
            serviceDescriptors.AddSingleton(typeof(JT809Consumer<JT809GpsPosition>), typeof(JT809_GpsPosition_Consumer));
            return serviceDescriptors;
        }
    }
}
