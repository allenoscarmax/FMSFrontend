using FMSFrontend.Models;
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
        private readonly GlobalProperties _global;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private const string DefaultScheme = "http";
        private const int DefaultPort = 5032;

        public HttpService(GlobalProperties global)
        {
            _httpClient = new HttpClient();
            _global = global;
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            // 初始化 ServerIp（從 INI 載入）
            var ini = new INIFile(AppDomain.CurrentDomain.BaseDirectory + "\\Basesitting.ini");
            var ip = ini.Read("Prarm", "IP");
            if (!string.IsNullOrWhiteSpace(ip))
            {
                // 僅設定 ServerIp，不修改 HttpClient 屬性，避免已發送請求後拋例外
                UpdateServerIp(ip);
            }
        }

        private bool ShouldBlockApiCall()
        {
            // 建議用你前面討論的規則：Open 或 HalfOpen 都擋（避免雪崩）
            if (_global.IsOpen) return true;
            if (_global.IsHalfOpen) return true;
            return false;
        }

        // 建立絕對 URL 字串（若 route 已是絕對 URL，則直接回傳）
        private string BuildUrl(string route)
        {
            if (string.IsNullOrWhiteSpace(route))
                throw new ArgumentException("route 不可為空白", nameof(route));

            // 若 route 已是絕對 URL，直接使用
            if (Uri.TryCreate(route, UriKind.Absolute, out var abs))
                return abs.ToString();

            if (string.IsNullOrWhiteSpace(ServerIp))
                throw new InvalidOperationException("尚未設定伺服器 IP");

            // 組合成絕對 URL
            var trimmed = route.TrimStart('/');
            return $"{DefaultScheme}://{ServerIp}:{DefaultPort}/{trimmed}";
        }

        public async Task SendGetAsync(string route)
        {
            var url = BuildUrl(route);
            await _httpClient.GetAsync(url).ConfigureAwait(false);
        }

        public async Task SendPostAsync<T>(string route, T payload)
        {
            var url = BuildUrl(route);
            await _httpClient.PostAsJsonAsync(url, payload, _jsonOptions).ConfigureAwait(false);
        }

        public async Task<bool> SendPutAsync<T>(string route, T payload)
        {
            if (ShouldBlockApiCall())
                return default;
            try
            { 
                var url = BuildUrl(route);
                using var resp = await _httpClient.PutAsJsonAsync(url, payload, _jsonOptions).ConfigureAwait(false);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task SendDeleteAsync(string route)
        {
            var url = BuildUrl(route);
            await _httpClient.DeleteAsync(url).ConfigureAwait(false);
        }

        // 改為有過濾的 GetJsonAsync：當回傳為 null / undefined / 空陣列 / 空字串 時回傳 null (default)
        public async Task<T?> GetJsonAsync<T>(string route, CancellationToken cancellationToken = default)
        {
            bool isHealth = route.StartsWith("/Health/", StringComparison.OrdinalIgnoreCase);

            if (!isHealth && ShouldBlockApiCall())
                return default;

            try {
                var url = BuildUrl(route);
                using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

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
            catch
            {
                return default;
            }
        }

        // 取得原始字串（不反序列化）
        public async Task<string?> GetJsonAsyncNoDeserialize(string route, CancellationToken cancellationToken = default)
        {
            if (ShouldBlockApiCall())
                return default;

            var url = BuildUrl(route);
            using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

            // 無內容或 204
            if (response.StatusCode == HttpStatusCode.NoContent || response.Content == null)
                return null;

            // 非成功狀態，不嘗試反序列化
            if (!response.IsSuccessStatusCode)
                return null;

            // 讀取為字串以便做額外判斷
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return string.IsNullOrWhiteSpace(content) ? null : content;
        }
        /*
        // PutJsonAsync：加入回傳過濾（與 GetJsonAsync 相同策略）
        //public async Task<TResult?> PutJsonAsync<TRequest, TResult>(string route, TRequest payload, CancellationToken cancellationToken = default)
        //{
        //    var url = BuildUrl(route);
        //    using var response = await _httpClient.PutAsJsonAsync(url, payload, _jsonOptions, cancellationToken).ConfigureAwait(false);

        //    // 無內容或 204
        //    if (response.StatusCode == HttpStatusCode.NoContent || response.Content == null)
        //        return default;

        //    // 非成功狀態，不嘗試反序列化（避免拋例外）
        //    if (!response.IsSuccessStatusCode)
        //        return default;

        //    // 讀取為字串以便做額外判斷
        //    var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        //    if (string.IsNullOrWhiteSpace(content))
        //        return default;

        //    try
        //    {
        //        // 若 TResult 是 JsonElement（或 Nullable<JsonElement>），先解析並檢查 ValueKind
        //        var targetType = typeof(TResult);
        //        var isJsonElement = targetType == typeof(JsonElement) || targetType == typeof(JsonElement?);

        //        if (isJsonElement)
        //        {
        //            using var doc = JsonDocument.Parse(content);
        //            var root = doc.RootElement;
        //            if (root.ValueKind == JsonValueKind.Null
        //                || root.ValueKind == JsonValueKind.Undefined
        //                || (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() == 0)
        //                || (root.ValueKind == JsonValueKind.String && string.IsNullOrWhiteSpace(root.GetString())))
        //            {
        //                return default;
        //            }

        //            object boxed = root.Clone();
        //            return (TResult?)boxed;
        //        }

        //        // 其他型別直接用 JsonSerializer 反序列化
        //        var result = JsonSerializer.Deserialize<TResult>(content, _jsonOptions);
        //        return result;
        //    }
        //    catch
        //    {
        //        // 若解析或反序列化失敗，回傳 default 以維持呼叫端相容性
        //        return default;
        //    }
        //}
        */
        public async Task<TResult?> PutJsonAsync<TRequest, TResult>(string route, TRequest payload, CancellationToken cancellationToken = default)
        {
            if (ShouldBlockApiCall()) return default;

            var url = BuildUrl(route);
            try
            {
                using var response = await _httpClient.PutAsJsonAsync(url, payload, _jsonOptions, cancellationToken).ConfigureAwait(false);

                // 若 content 為空 → 直接回傳 default
                if (response.Content == null)
                    return default;

                var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(content))
                    return default;

                try
                {
                    return JsonSerializer.Deserialize<TResult>(content, _jsonOptions);
                }
                catch
                {
                    return default;
                }
            }
            catch { return default; }
        }

        public string? ServerIp { get; private set; }

        public void UpdateServerIp(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                throw new ArgumentException("IP 不可為空白", nameof(ip));
            else if (!(IPAddress.TryParse(ip, out _) || ip == "localhost"))
                throw new ArgumentException("IP 位址格式不正確", nameof(ip));

            // 不再修改 HttpClient.BaseAddress；僅更新 ServerIp
            ServerIp = ip;
        }
    }
}
