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

        public async Task SendGetAsync(string route)
        {
            await _httpClient.GetAsync(route).ConfigureAwait(false);
        }

        public async Task SendPostAsync<T>(string route, T payload)
        {
            await _httpClient.PostAsJsonAsync(route, payload, _jsonOptions).ConfigureAwait(false);
        }

        public async Task<bool> SendPutAsync<T>(string route, T payload)
        {
            try
            {
                using var resp = await _httpClient.PutAsJsonAsync(route, payload, _jsonOptions).ConfigureAwait(false);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task SendDeleteAsync(string route)
        {
            await _httpClient.DeleteAsync(route).ConfigureAwait(false);
        }

        // 改為有過濾的 GetJsonAsync：當回傳為 null / undefined / 空陣列 / 空字串 時回傳 null (default)
        public async Task<T?> GetJsonAsync<T>(string route, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync(route, cancellationToken).ConfigureAwait(false);

            // 無內容或 204
            if (response.StatusCode == HttpStatusCode.NoContent || response.Content == null)
                return default;

            // 非成功狀態，不嘗試反序列化
            if (!response.IsSuccessStatusCode)
                return default;

            // 讀取為字串以便做額外判斷
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(content))
                return default;

            var result = JsonSerializer.Deserialize<T>(content, _jsonOptions);
            return result;
        }

        // PutJsonAsync：加入回傳過濾（與 GetJsonAsync 相同策略）
        public async Task<TResult?> PutJsonAsync<TRequest, TResult>(string route, TRequest payload, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PutAsJsonAsync(route, payload, _jsonOptions, cancellationToken).ConfigureAwait(false);

            // 無內容或 204
            if (response.StatusCode == HttpStatusCode.NoContent || response.Content == null)
                return default;

            // 非成功狀態，不嘗試反序列化（避免拋例外）
            if (!response.IsSuccessStatusCode)
                return default;

            // 讀取為字串以便做額外判斷
            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(content))
                return default;

            try
            {
                // 若 TResult 是 JsonElement（或 Nullable<JsonElement>），先解析並檢查 ValueKind
                var targetType = typeof(TResult);
                var isJsonElement = targetType == typeof(JsonElement) || targetType == typeof(JsonElement?);

                if (isJsonElement)
                {
                    using var doc = JsonDocument.Parse(content);
                    var root = doc.RootElement;
                    if (root.ValueKind == JsonValueKind.Null
                        || root.ValueKind == JsonValueKind.Undefined
                        || (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() == 0)
                        || (root.ValueKind == JsonValueKind.String && string.IsNullOrWhiteSpace(root.GetString())))
                    {
                        return default;
                    }

                    object boxed = root.Clone();
                    return (TResult?)boxed;
                }

                // 其他型別直接用 JsonSerializer 反序列化
                var result = JsonSerializer.Deserialize<TResult>(content, _jsonOptions);
                return result;
            }
            catch
            {
                // 若解析或反序列化失敗，回傳 default 以維持呼叫端相容性
                return default;
            }
        }

        public string? ServerIp { get; private set; }

        public void UpdateServerIp(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                throw new ArgumentException("IP 不可為空白", nameof(ip));
            else if (!(IPAddress.TryParse(ip, out _) || ip == "localhost"))
                throw new ArgumentException("IP 位址格式不正確", nameof(ip));
            _httpClient.BaseAddress = new Uri($"http://{ip}:5032/");
            ServerIp = ip;
        }
    }
}
