using JT809.DotNetty.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.DotNetty.Core.Internal
{
    internal class VerifyCodeGeneratorDefaultImpl : IVerifyCodeGenerator
    {
        public uint Create()
        {
            return (uint)Guid.NewGuid().GetHashCode();
        }
    }
}
