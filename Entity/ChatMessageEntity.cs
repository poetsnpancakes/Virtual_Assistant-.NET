namespace Virtual_Assistant.Entity
{
    public class ChatMessageEntity
    {
        public int Id { get; set; }
        public string SessionId { get; set; } // Can be user ID or browser session ID
        public string Role { get; set; } // "user" or "assistant"
        public string Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

}
