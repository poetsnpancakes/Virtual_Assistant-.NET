using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qdrant.Client
{
    public class QdrantPoint
    {
        public int id { get; set; }
        public float[] vector { get; set; }
        public Dictionary<string, object> payload { get; set; }
    }
}
