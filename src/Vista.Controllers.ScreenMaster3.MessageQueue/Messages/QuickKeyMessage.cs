using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Vista.Controllers.ScreenMaster3.MessageQueue.Messages
{
    public class QuickKeyButton
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("color")]
        public QuickKeyColor Color { get; set; }
    }

    public class QuickKeyMessage
    {
        [JsonPropertyName("buttons")]
        public List<QuickKeyButton> Buttons { get; set; } = new List<QuickKeyButton>();
    }
}
