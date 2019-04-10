using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.PubSub.Abstractions
{
    /// <summary>
    /// jt809生产者分区工厂
    /// 分区策略：
    /// 1.可以根据（车牌号+颜色）进行分区
    /// 2.可以根据msgId(消息Id)+（车牌号+颜色）进行分区
    /// </summary>
    public interface IJT809ProducerPartitionFactory
    {
        int CreatePartition(string topicName, string msgId, string vno_color);
    }
}
