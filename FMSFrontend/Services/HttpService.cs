using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FMSFrontend.Services
{
    public class HttpService : IHttpService
    {
        private readonly HttpClient _httpClient;

        public HttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task SendGetAsync(string route)
        {
            await _httpClient.GetAsync(route);
        }

        public async Task SendPostAsync<T>(string route, T payload)
        {
            await _httpClient.PostAsJsonAsync(route, payload);
        }

        public async Task SendPutAsync<T>(string route, T payload)
        {
            await _httpClient.PutAsJsonAsync(route, payload);
        }

        public async Task SendDeleteAsync(string route)
        {
            await _httpClient.DeleteAsync(route);
        }

        // 新增：直接取得並反序列化 JSON
        public Task<T?> GetJsonAsync<T>(string route, CancellationToken cancellationToken = default)
            => _httpClient.GetFromJsonAsync<T>(route, cancellationToken);
    }
}
