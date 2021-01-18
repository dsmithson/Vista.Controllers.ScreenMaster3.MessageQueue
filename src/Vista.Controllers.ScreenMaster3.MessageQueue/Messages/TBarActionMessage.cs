using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Vista.Controllers.ScreenMaster3.MessageQueue.Messages
{
    public class TBarActionMessage
    {
        [JsonPropertyName("tbarPosition")]
        public int TBarPosition { get; set; }
    }
}
