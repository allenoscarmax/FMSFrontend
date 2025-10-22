using System.Threading;
using System.Threading.Tasks;

namespace FMSFrontend.Services
{
    public interface IHttpService
    {
        Task SendGetAsync(string route);
        Task SendPostAsync<T>(string route, T payload);
        Task<bool> SendPutAsync<T>(string route, T payload);
        Task SendDeleteAsync(string route);

        // 取得並反序列化 JSON（GET）
        Task<T?> GetJsonAsync<T>(string route, CancellationToken cancellationToken = default);

        //送出 PUT JSON，並反序列化回傳 JSON
        Task<TResult?> PutJsonAsync<TRequest, TResult>(string route, TRequest payload, CancellationToken cancellationToken = default);

        // 目前設定的伺服器 IP（若尚未設定可能為 null/空字串）
        string? ServerIp { get; }

        // 更新伺服器 IP，並依現有 BaseAddress 的 scheme/port 重建 BaseAddress
        void UpdateServerIp(string ip);
    }
}
