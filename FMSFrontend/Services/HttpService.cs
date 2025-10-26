using IniFile;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FMSFrontend.Services
{
    public class HttpService : IHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public HttpService()
        {
            _httpClient = new HttpClient();

            // 初始化 ServerIp（從 INI 載入）
            var ini = new INIFile(AppDomain.CurrentDomain.BaseDirectory + "\\Basesitting.ini");
            var ip = ini.Read("Prarm", "IP");
            if (!string.IsNullOrWhiteSpace(ip))
            {
                UpdateServerIp(ip);
            }
        }

        public async Task SendGetAsync(string route) => await _httpClient.GetAsync(route);

        public async Task SendPostAsync<T>(string route, T payload) => await _httpClient.PostAsJsonAsync(route, payload, _jsonOptions);

        public async Task<bool> SendPutAsync<T>(string route, T payload)
        {
            try
            {
                using var resp = await _httpClient.PutAsJsonAsync(route, payload, _jsonOptions);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task SendDeleteAsync(string route) => await _httpClient.DeleteAsync(route);

        public Task<T?> GetJsonAsync<T>(string route, CancellationToken cancellationToken = default)
            => _httpClient.GetFromJsonAsync<T>(route, _jsonOptions, cancellationToken);

        public async Task<TResult?> PutJsonAsync<TRequest, TResult>(string route, TRequest payload, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PutAsJsonAsync(route, payload, _jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();

            if (response.StatusCode == HttpStatusCode.NoContent || response.Content == null)
                return default;

            return await response.Content.ReadFromJsonAsync<TResult>(options: _jsonOptions, cancellationToken: cancellationToken);
        }

        public string? ServerIp { get; private set; }

        public void UpdateServerIp(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                throw new ArgumentException("IP 不可為空白", nameof(ip));

            ServerIp = ip;

            _httpClient.BaseAddress = new Uri($"http://{ip}:5032/");
        }
    }
}
