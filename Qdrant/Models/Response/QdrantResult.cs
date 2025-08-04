namespace Qdrant.Models.Response
{
    public class QdrantResult
    {
        public string Id { get; set; }
        public float Score { get; set; }
        public Dictionary<string, object> Payload { get; set; }
    }
}
