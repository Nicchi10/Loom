using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Loom.Engine.Infrastructure
{
    public class ProviderHttpClient
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public async Task<TResponse> PostAsync<TResponse>(Dictionary<string, string> headers, string url, Dictionary<string, object> payload)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            foreach (var h in headers)
                _httpClient.DefaultRequestHeaders.Add(h.Key, h.Value);

            var serializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            string jsonBody = JsonConvert.SerializeObject(payload, serializerSettings);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(url, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Provider error {response.StatusCode}: {responseBody}");

                TResponse modelResponse = JsonConvert.DeserializeObject<TResponse>(responseBody);
                return modelResponse;
            }
            catch (Exception ex)
            {
                throw new Exception($"[ERROR] while network calling to {url}: {ex}");
            }
        }
    }
}
