using JT809.DotNetty.Core.Metadata;

namespace JT809.DotNetty.Core.Services
{
    /// <summary>
    /// Tcp计数包服务
    /// </summary>
    public class JT809TcpAtomicCounterService
    {
        private readonly JT809AtomicCounter MsgSuccessCounter = new JT809AtomicCounter();

        private readonly JT809AtomicCounter MsgFailCounter = new JT809AtomicCounter();

        public JT809TcpAtomicCounterService()
        {

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
