using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Loom.Engine.Infrastructure
{
    public class ProviderHttpClient
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        private static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true
        };

        public async Task<TResponse> PostAsync<TResponse>(Dictionary<string, string> headers, string url, Dictionary<string, object> payload)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            foreach (var h in headers)
                _httpClient.DefaultRequestHeaders.Add(h.Key, h.Value);

            string jsonBody = JsonSerializer.Serialize(payload, _serializerOptions);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(url, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Provider error {response.StatusCode}: {responseBody}");

                TResponse modelResponse = JsonSerializer.Deserialize<TResponse>(responseBody, _serializerOptions);
                return modelResponse;
            }
            catch (Exception ex)
            {
                throw new Exception($"[ERROR] while network calling to {url}: {ex}");
            }
        }
    }
}
