using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.DotNetty.Core.Interfaces
{
    public interface IJT809ManualResetEvent
    {
        void Pause();
        bool Resume();
        bool Reset();
    }
}
