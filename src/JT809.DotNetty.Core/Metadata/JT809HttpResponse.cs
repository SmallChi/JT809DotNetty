using System;
using System.Collections.Generic;
using System.Reflection;

namespace JT809.DotNetty.Core.Metadata
{
    public class JT809HttpResponse
    {
        public byte[] Data { get; set; }

        public JT809HttpResponse()
        {

        }

        public JT809HttpResponse(byte[] data)
        {
            this.Data = data;
        }
    }
}