using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Vista.Controllers.ScreenMaster3.MessageQueue.Messages
{
    public class RotaryActionMessage
    {
        [JsonPropertyName("rotaryIndex")]
        public int RotaryIndex { get; set; }

        [JsonPropertyName("rotaryOffset")]
        public int RotaryOffset { get; set; }
    }
}
