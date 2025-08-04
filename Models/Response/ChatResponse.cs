namespace Virtual_Assistant.Models.Response
{
    public class ChatResponse
    {
        public string Role { get; set; } // "user" or "assistant"
        public string Content { get; set; }
    }
}
