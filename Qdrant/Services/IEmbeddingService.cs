using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qdrant.Services
{
    public interface IEmbeddingService
    {
        Task<float[]> GetEmbeddingAsync(string input);
    }
}
