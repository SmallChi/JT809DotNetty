using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;

namespace JT809.DotNetty.Core.Services
{
    public  class JT809AtomicCounterServiceFactory
    {
        private static readonly ConcurrentDictionary<string, JT809AtomicCounterService> cache;

        static JT809AtomicCounterServiceFactory()
        {
            cache = new ConcurrentDictionary<string, JT809AtomicCounterService>(StringComparer.OrdinalIgnoreCase);
        }

        public JT809AtomicCounterService Create(string type)
        {
            if(cache.TryGetValue(type,out var service))
            {
                return service;
            }
            else
            {
                var serviceNew = new JT809AtomicCounterService();
                cache.TryAdd(type, serviceNew);
                return serviceNew;
            }
        }
    }
}
