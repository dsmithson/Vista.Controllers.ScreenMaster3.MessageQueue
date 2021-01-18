using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Vista.Controllers.ScreenMaster3.MessageQueue.Messages
{
    public enum LampCommandType 
    {  
        [JsonPropertyName("clearAll")]
        ClearAll,

        [JsonPropertyName("setOn")]
        SetOn,

        [JsonPropertyName("setOff")]
        SetOff 
    }

    public class LampCommandMessage
    {
        [JsonPropertyName("command")]
        public LampCommandType Command { get; set; }

        [JsonPropertyName("keyIndexes")]
        public List<int> KeyIndexes { get; set; } = new List<int>();
    }
}
