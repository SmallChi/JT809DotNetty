using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JT809.KafkaService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JT809.KafkaServiceTest
{
    public class TestConsumerPartitionBase
    {
        public IServiceProvider ServiceProvider { get; }

        public IConfigurationRoot ConfigurationRoot { get; }
        public TestConsumerPartitionBase()
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
            builder.AddJsonFile("appsettings.Partition.json");
            ConfigurationRoot = builder.Build();

            ServiceCollection serviceDescriptors = new ServiceCollection();
            serviceDescriptors.AddSingleton<ILoggerFactory, LoggerFactory>();
            serviceDescriptors.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            serviceDescriptors.AddLogging(configure =>
            {
                configure.AddDebug();
                configure.SetMinimumLevel(LogLevel.Trace);
            });
            serviceDescriptors.AddJT809KafkaConsumerPartitionsService(ConfigurationRoot,partition=> { partition.Partition = 4; });
            serviceDescriptors.AddSingleton<ConsumerTestPartitionService>();
            ServiceProvider = serviceDescriptors.BuildServiceProvider();
        }
    }
}
