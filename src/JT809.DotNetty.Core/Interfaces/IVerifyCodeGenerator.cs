using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.DotNetty.Core.Interfaces
{
    /// <summary>
    /// 校验码生成器
    /// </summary>
    public  interface IVerifyCodeGenerator
    {
        uint Create();
    }
}
