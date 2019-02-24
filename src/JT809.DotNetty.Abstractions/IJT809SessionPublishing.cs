using System.Threading.Tasks;

namespace JT809.DotNetty.Abstractions
{
    /// <summary>
    /// 会话通知（在线/离线）
    /// </summary>
    public interface IJT809SessionPublishing
    {
        Task PublishAsync(string topicName, string value);
    }
}
