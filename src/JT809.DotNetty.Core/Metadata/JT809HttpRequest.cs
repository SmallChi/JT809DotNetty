using System;
using System.Collections.Generic;
using System.Reflection;

namespace JT809.DotNetty.Core.Metadata
{
    public class JT809HttpRequest
    {
        public string Json { get; set; }

        public JT809HttpRequest()
        {

        }

        public JT809HttpRequest(string json)
        {
            Json = json;
        }
    }
}