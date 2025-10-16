using System.Threading;
using System.Threading.Tasks;

namespace FMSFrontend.Services
{
    public interface IHttpService
    {
        Task SendGetAsync(string route);
        Task SendPostAsync<T>(string route, T payload);
        Task SendPutAsync<T>(string route, T payload);
        Task SendDeleteAsync(string route);

        // 新增：直接取得並反序列化 JSON
        Task<T?> GetJsonAsync<T>(string route, CancellationToken cancellationToken = default);
    }
}
