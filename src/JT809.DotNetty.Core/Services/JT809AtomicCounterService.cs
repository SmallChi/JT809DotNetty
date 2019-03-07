using JT809.DotNetty.Core.Metadata;

namespace JT809.DotNetty.Core.Services
{
    /// <summary>
    /// Tcp计数包服务
    /// </summary>
    internal class JT809AtomicCounterService
    {
        private readonly JT809AtomicCounter MsgSuccessCounter;

        private readonly JT809AtomicCounter MsgFailCounter;

        public JT809AtomicCounterService()
        {
            MsgSuccessCounter=new JT809AtomicCounter();
            MsgFailCounter = new JT809AtomicCounter();
        }

        public void Reset()
        {
            MsgSuccessCounter.Reset();
            MsgFailCounter.Reset();
        }

        public long MsgSuccessIncrement()
        {
            return MsgSuccessCounter.Increment();
        }

        public long MsgSuccessCount
        {
            get
            {
                return MsgSuccessCounter.Count;
            }
        }

        public long MsgFailIncrement()
        {
            return MsgFailCounter.Increment();
        }

        public long MsgFailCount
        {
            get
            {
                return MsgFailCounter.Count;
            }
        }
    }
}
