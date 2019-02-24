using JT809.DotNetty.Abstractions;
using System.Threading.Tasks;

namespace JT809.DotNetty.Core
{
    internal class JT809SessionPublishingEmptyImpl : IJT809SessionPublishing
    {
        public Task PublishAsync(string topicName, string value)
        {
            return Task.CompletedTask;
        }
    }
}
