using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Vista.Controllers.ScreenMaster3.MessageQueue.Messages
{
    public class KeyActionMessage
    {
        [JsonPropertyName("keyIndex")]
        public int KeyIndex { get; set; }

        [JsonPropertyName("isPressed")]
        public bool IsPressed { get; set; }
    }
}
