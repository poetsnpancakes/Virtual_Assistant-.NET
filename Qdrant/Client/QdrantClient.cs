using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Qdrant.Models.Response;
using Qdrant.Models.Request;
using Qdrant.Services;
using Microsoft.Data.SqlClient;
using Dapper;


namespace Qdrant.Client
{
    public class QdrantClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        //private readonly ILogger<QdrantClient> _logger;
        private readonly IEmbeddingService _openAiService;


        public QdrantClient(HttpClient httpClient, IConfiguration config, IEmbeddingService openAiService)
        {
            _httpClient = httpClient;
            _config = config;
            _openAiService = openAiService;
            
        }

        public async Task<bool> RecreateCollectionAsync(string collectionName, int vectorSize = 1536)
        {
            // Step 1: Delete collection if exists
            await _httpClient.DeleteAsync($"{_config["Qdrant_local"]}/collections/{collectionName}");

            // Step 2: Create new collection with config
            var request = new
            {
                vectors = new
                {
                    size = vectorSize,
                    distance = "Cosine"
                }
            };

            var response = await _httpClient.PutAsJsonAsync($"{_config["Qdrant_local"]}/collections/{collectionName}", request);
            return response.IsSuccessStatusCode;
        }
    

    public async Task CreateCollectionAsync(string collectionName, int vectorSize)
        {
           
            var requestBody = new
            {
                vectors = new { size = vectorSize, distance = "Cosine" }
            };

            var response = await _httpClient.PutAsJsonAsync($"{_config["Qdrant_local"]}/collections/{collectionName}", requestBody);
            response.EnsureSuccessStatusCode();
        }

        public async Task InsertVectorAsync(string collectionName, string id, float[] vector, object payload = null)
        {
            var requestBody = new
            {
                points = new[]
                {
                new {
                    id,
                    vector,
                    payload
                }
            }
            };

            var response = await _httpClient.PutAsJsonAsync($"{_config["Qdrant_local"]}/collections/{collectionName}/points", requestBody);
            response.EnsureSuccessStatusCode();
        }

        //public async Task<List<QdrantResult>> SearchAsync(string collectionName, float[] queryVector, int topK = 5)
        //{
        //    var requestBody = new
        //    {
        //        vector = queryVector,
        //        top = topK,
        //        with_payload = true
        //    };

        //    var response = await _httpClient.PostAsJsonAsync(
        //        $"{_config["Qdrant_local"]}/collections/{collectionName}/points/search", requestBody);

        //    response.EnsureSuccessStatusCode();

        //    var stream = await response.Content.ReadAsStreamAsync();
        //    var doc = await JsonDocument.ParseAsync(stream);

        //    var results = new List<QdrantResult>();

        //    if (doc.RootElement.ValueKind == JsonValueKind.Array)
        //    {
        //        foreach (var item in doc.RootElement.EnumerateArray())
        //        {
        //            results.Add(ParseQdrantResult(item));
        //        }
        //    }

        //    return results;
        //}

        public async Task<List<QdrantResult>> SearchAsync(string collectionName, float[] queryVector, int topK = 5)
        {
            var requestBody = new
            {
                vector = queryVector,
                top = topK,
                with_payload = true,
                with_vector = false // set to true if you want vectors returned
            };

            try
            {
                var url = $"{_config["Qdrant_local"]}/collections/{collectionName}/points/search";
                var response = await _httpClient.PostAsJsonAsync(url, requestBody);
                response.EnsureSuccessStatusCode();

                var stream = await response.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);

                var results = new List<QdrantResult>();

                if (doc.RootElement.TryGetProperty("result", out var resultElement) &&
                    resultElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in resultElement.EnumerateArray())
                    {
                        results.Add(ParseQdrantResult(item));
                    }
                }
                else
                {
                    Console.WriteLine("⚠️ No 'result' array found in Qdrant response.");
                }

                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Qdrant search error for collection '{collectionName}': {ex.Message}");
                return new List<QdrantResult>();
            }
        }



        //private QdrantResult ParseQdrantResult(JsonElement item)
        //{
        //    var id = item.GetProperty("id").ToString();
        //    var score = item.GetProperty("score").GetSingle();
        //    var payload = JsonSerializer.Deserialize<Dictionary<string, object>>(item.GetProperty("payload").GetRawText());


        //    return new QdrantResult
        //    {
        //        Id = id,
        //        Score = score,
        //        Payload = payload
        //    };
        //}

        private QdrantResult ParseQdrantResult(JsonElement item)
        {
            string id = item.TryGetProperty("id", out var idElement) ? idElement.ToString() : "unknown";

            float score = 0f;
            if (item.TryGetProperty("score", out var scoreElement))
            {
                if (scoreElement.TryGetSingle(out float s))
                    score = s;
                else if (scoreElement.TryGetDouble(out double d))
                    score = (float)d;
            }

            Dictionary<string, object>? payload = null;
            if (item.TryGetProperty("payload", out var payloadElement) && payloadElement.ValueKind == JsonValueKind.Object)
            {
                payload = JsonSerializer.Deserialize<Dictionary<string, object>>(payloadElement.GetRawText());
            }

            return new QdrantResult
            {
                Id = id,
                Score = score,
                Payload = payload ?? new Dictionary<string, object>()
            };
        }



        //public async Task<List<Dictionary<string, object>>> FetchTableAsync(string tableName)
        //{
        //    try
        //    {
        //        string connectionString = _config["CONNECTION_STRING_ENV"];
        //        using var conn = new SqlConnection(connectionString);

        //        await conn.OpenAsync(); // ← will throw if connection fails

        //        var query = $"SELECT * FROM {tableName}";
        //        var result = await conn.QueryAsync(query);

        //        return result.Select(r => (IDictionary<string, object>)r)
        //                     .Select(d => d.ToDictionary(k => k.Key, v => v.Value))
        //                     .ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"❌ FetchTableAsync failed for '{tableName}': {ex.Message}");
        //        throw; // Rethrow so you can catch it in Razor page
        //    }
        //}



        public async Task<List<(int Id, string Text)>> GetTextDataFromMSSQL(string connectionString, string tableName)
        {
            var data = new List<(int, string)>();
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            var query = $"SELECT * FROM {tableName}";
            using var cmd = new SqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                data.Add((reader.GetInt32(0), reader.GetString(1)));
            }

            return data;
        }

        //public async Task<List<(int Id, string Text)>> GetTextDataFromMSSQL(string connectionString, string tableName)
        //{
        //    //var allowedTables = new[] { "Careers", "ServicesOffereds", "DirectorsInfo" };

        //    //if (!allowedTables.Contains(tableName))
        //    //    throw new ArgumentException($"❌ Invalid table name: '{tableName}'");

        //    var data = new List<(int, string)>();

        //    try
        //    {
        //        using var conn = new SqlConnection(connectionString);
        //        await conn.OpenAsync();

        //        var query = $"SELECT * FROM [{tableName}]"; // Safe with table name whitelist
        //        using var cmd = new SqlCommand(query, conn);
        //        using var reader = await cmd.ExecuteReaderAsync();

        //        while (await reader.ReadAsync())
        //        {
        //            // Ensure data is not NULL and type-safe
        //            var id = reader.IsDBNull(0) ? -1 : reader.GetInt32(0);
        //            var text = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);

        //            data.Add((id, text));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"❌ Failed to fetch from '{tableName}': {ex.Message}");
        //        throw;
        //    }

        //    return data;
        //}



        public async Task<bool> CreateQdrantCollectionAsync(string collectionName, int vectorSize)
        {

            
            await _httpClient.DeleteAsync($"{_config["Qdrant_local"]}/collections/{collectionName}");

            var body = new
            {
                vectors = new
                {
                    size = vectorSize,
                    distance = "Cosine"
                }
            };

            //var response = await _httpClient.PutAsJsonAsync($"{_config["Qdrant_local"]}/collections/{collectionName.ToLower()}/points", body);
            var response = await _httpClient.PutAsJsonAsync($"{_config["Qdrant_local"]}/collections/{collectionName}", body);
            return response.IsSuccessStatusCode; 
        }



        //public async Task<bool> UploadTableToQdrantAsync(string tableName)
        //{
        //    try
        //    {
        //        Console.WriteLine($"📥 Fetching records from table: {tableName}");

        //        var records = await FetchTableAsync(tableName);
        //        if (records == null || !records.Any())
        //        {
        //            Console.WriteLine($"❌ No records found in table '{tableName}'");
        //            return false;
        //        }

        //        var points = new List<QdrantPoint>();
        //        int id = 0;

        //        foreach (var record in records)
        //        {
        //            // Combine all string fields
        //            string combined = string.Join(" ", record
        //                     .Where(kv => kv.Value is string && !string.IsNullOrWhiteSpace((string)kv.Value))
        //                     .Select(kv => kv.Value));

        //            if (string.IsNullOrWhiteSpace(combined)) continue;

        //            // Get embedding from OpenAI
        //            float[] embedding = await _openAiService.GetEmbeddingAsync(combined);

        //            if (embedding == null || embedding.Length != 1536)
        //            {
        //                Console.WriteLine($"❌ Invalid embedding length: {embedding?.Length ?? 0}");
        //                continue;
        //            }

        //            Console.WriteLine($"✅ Got embedding for record #{id} - length: {embedding.Length}");

        //            var point = new QdrantPoint
        //            {
        //                id = id++,
        //                vector = embedding,
        //                payload = record
        //            };

        //            points.Add(point);
        //        }

        //        if (!points.Any())
        //        {
        //            Console.WriteLine($"❌ No valid points to upload for table '{tableName}'");
        //            return false;
        //        }

        //        Console.WriteLine($"📦 Preparing to upload {points.Count} points to Qdrant");

        //        var requestBody = new
        //        {
        //            points = points.Select(p => new
        //            {
        //                id = p.id,
        //                vector = p.vector,
        //                payload = p.payload
        //            }).ToList()
        //        };

        //        var response = await _httpClient.PostAsJsonAsync(
        //            $"{_config["Qdrant_local"]}/collections/{tableName.ToLower()}/points", requestBody
        //        );

        //        string responseContent = await response.Content.ReadAsStringAsync();

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            Console.WriteLine($"❌ Qdrant upload failed: {response.StatusCode}");
        //            Console.WriteLine($"📄 Response content: {responseContent}");
        //            return false;
        //        }

        //        Console.WriteLine($"✅ Successfully uploaded to '{tableName.ToLower()}'");
        //        Console.WriteLine($"📄 Qdrant Response: {responseContent}");

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"❌ Exception while uploading '{tableName}': {ex.Message}");
        //        return false;
        //    }
        //}



        public async Task UploadVectorToQdrantAsync(string collectionName, int id, float[] vector, string description)
        {
            
            //var url = $"http://localhost:6333/collections/{collectionName}/points";

            var body = new
            {
                points = new[]
                {
            new
            {
                id = id,
                vector = vector,
                payload = new
                {
                    description = description
                }
            }
              }
            };

            var response = await _httpClient.PutAsJsonAsync($"{_config["Qdrant_local"]}/collections/{collectionName}", body);
            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> TransferMSSQLDataToQdrant(string collectionName)
        {
            try
            {
                string connectionString = _config["CONNECTION_STRING_ENV"];
                //string collectionName = "careers";
                int vectorSize = 1536; // OpenAI text-embedding-3-small output size

                // Step 1: Fetch data from MSSQL
                var data = await GetTextDataFromMSSQL(connectionString,collectionName);
                if (data == null || data.Count == 0)
                {
                    Console.WriteLine("No data found in MSSQL.");
                    return false;
                }

                // Step 2: Create collection in Qdrant
                await CreateQdrantCollectionAsync(collectionName, vectorSize);

                var points = new List<QdrantPoint>();

                // Step 3: Loop through data, get embeddings, and upload to Qdrant
                foreach (var (id, text) in data)
                {
                    try
                    {
                        var vector = await _openAiService.GetEmbeddingAsync(text);
                        await UploadVectorToQdrantAsync(collectionName, id, vector, text);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed for ID {id}: {ex.Message}");
                    }
                }

                Console.WriteLine("Data transfer to Qdrant completed successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
                return false;
            }
        }




    }
}
