using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.Superior.Server.Configs
{
    public class JT809GpsOptions : IOptions<JT809GpsOptions>
    {
        public string FromChannel { get; set; }
        public JT809GpsOptions Value =>this;
    }
}
