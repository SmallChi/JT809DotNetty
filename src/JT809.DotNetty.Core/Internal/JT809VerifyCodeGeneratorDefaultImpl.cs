using JT809.DotNetty.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.DotNetty.Core.Internal
{
    internal class JT809VerifyCodeGeneratorDefaultImpl : IJT809VerifyCodeGenerator
    {
        private uint VerifyCode;

        public uint Create()
        {
            VerifyCode= (uint)Guid.NewGuid().GetHashCode();
            return VerifyCode;
        }

        public uint Get()
        {
            return VerifyCode;
        }
    }
}
