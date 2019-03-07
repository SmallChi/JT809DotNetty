using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.DotNetty.Core.Interfaces
{
    /// <summary>
    /// 校验码生成器
    /// 注:上级平台使用
    /// </summary>
    public interface IJT809VerifyCodeGenerator
    {
        uint Create();
        uint Get();
    }
}
