using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.PubSub.Abstractions
{
    public class JT809PartitionOptions:IOptions<JT809PartitionOptions>
    {
        public int Partition { get; set; } = 1;

        public JT809PartitionOptions Value => this;

        public List<int> AssignPartitions { get; set; }
    }
}
