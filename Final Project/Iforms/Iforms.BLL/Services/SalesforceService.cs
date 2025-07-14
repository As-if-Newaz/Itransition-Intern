using Iforms.BLL.DTOs;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Iforms.BLL.Services
{
    public class SalesforceService
    {
        private readonly HttpClient httpClient;
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string username;
        private readonly string password;
        private readonly string tokenEndpoint;
        private readonly string apiBaseUrl;

        public SalesforceService(HttpClient httpClient, string clientId, string clientSecret, string username, string password, string tokenEndpoint, string apiBaseUrl)
        {
            this.httpClient = httpClient;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.username = username;
            this.password = password;
            this.tokenEndpoint = tokenEndpoint;
            this.apiBaseUrl = apiBaseUrl;
        }

        public async Task<string?> AuthenticateAsync()
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password)
            });

            var response = await httpClient.PostAsync(tokenEndpoint, content);
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("access_token", out var token) ? token.GetString() : null;
        }

        public async Task<string?> CreateAccountAsync(SalesforceAccountDTO dto, string accessToken)
        {
            var url = apiBaseUrl + "sobjects/Account";
            var json = JsonSerializer.Serialize(dto);
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await httpClient.SendAsync(request);
            var respJson = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) return null;
            using var doc = JsonDocument.Parse(respJson);
            return doc.RootElement.TryGetProperty("id", out var id) ? id.GetString() : null;
        }

        public async Task<string?> CreateContactAsync(SalesforceContactDTO dto, string accessToken)
        {
            var url = apiBaseUrl + "sobjects/Contact";
            var json = JsonSerializer.Serialize(dto);
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await httpClient.SendAsync(request);
            var respJson = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) return null;
            using var doc = JsonDocument.Parse(respJson);
            return doc.RootElement.TryGetProperty("id", out var id) ? id.GetString() : null;
        }
    }
} 