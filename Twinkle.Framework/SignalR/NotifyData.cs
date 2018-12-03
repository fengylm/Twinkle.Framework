using System;

namespace Twinkle.Framework.SignalR
{
    [Serializable]
    public class NotifyData
    {
        public string Type { get; set; }

        public object Data { get; set; }

    }
}
