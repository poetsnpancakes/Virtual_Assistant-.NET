using System.Text.Json.Serialization;

namespace Virtual_Assistant.Models.Response
{
    public class VoiceMessage
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("voicePrompt")]
        public string VoicePrompt { get; set; }

        [JsonPropertyName("lang")]
        public string Lang { get; set; }

        [JsonPropertyName("last")]
        public bool Last { get; set; }
    }

}
