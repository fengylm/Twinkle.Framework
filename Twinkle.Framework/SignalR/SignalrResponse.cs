using System;
using System.Collections.Generic;
using System.Text;

namespace Twinkle.Framework.SignalR
{
    public class SignalrResponse
    {
        public string Channel { get; set; }

        public object Body { get; set; }
    }
}
