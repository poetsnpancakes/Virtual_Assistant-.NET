using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qdrant.Client;

namespace Qdrant.Models.Request
{
    public class QdrantCollectionRequest
    {
        public QdrantVectorParams vectors { get; set; }
    }
}
