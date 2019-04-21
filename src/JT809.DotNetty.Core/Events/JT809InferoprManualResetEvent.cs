using JT809.DotNetty.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace JT809.DotNetty.Core.Events
{
    public class JT809InferoprManualResetEvent: IJT809ManualResetEvent
    {
        private ManualResetEvent ManualResetEvent;

        public JT809InferoprManualResetEvent()
        {
            ManualResetEvent = new ManualResetEvent(false);
        }

        public void Pause()
        {
            ManualResetEvent.WaitOne();
        }

        public bool Reset()
        {
            return ManualResetEvent.Reset();
        }

        public bool Resume()
        {
            return ManualResetEvent.Set();
        }
    }
}
