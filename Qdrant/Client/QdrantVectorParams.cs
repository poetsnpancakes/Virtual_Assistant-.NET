using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qdrant.Client
{
    public class QdrantVectorParams
    {
        public int size { get; set; }
        public string distance { get; set; } = "Cosine"; // or "Euclidean", "Dot"
    }
}
