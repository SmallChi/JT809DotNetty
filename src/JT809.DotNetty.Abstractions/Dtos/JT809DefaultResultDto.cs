using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.DotNetty.Abstractions.Dtos
{
    public class JT809DefaultResultDto: JT809ResultDto<string>
    {
        public JT809DefaultResultDto()
        {
            Data = "Hello,JT809 Superior WebAPI";
            Code = JT809ResultCode.Ok;
        }
    }
}
