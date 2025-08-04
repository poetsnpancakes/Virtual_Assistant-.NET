namespace Virtual_Assistant.Models.Response
{
    public class BotResponse
    {
        public string Query { get; set; }
        public string Route { get; set; }
        public string RephrasedQuery { get; set; }
        public string Answer { get; set; }
    }
}
