namespace Virtual_Assistant.Models.Request
{
    public class ChatMessage
    {
        public string Role { get; set; } // "user" or "assistant"
        public string Content { get; set; }
    }

}
