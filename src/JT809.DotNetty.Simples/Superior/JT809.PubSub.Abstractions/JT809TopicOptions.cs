using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace JT809.PubSub.Abstractions
{
    public class JT809TopicOptions:IOptions<JT809TopicOptions>
    {
        public string TopicName { get; set; } = JT809Constants.JT809TopicName;

        public JT809TopicOptions Value => this;
    }
}
