using FMSFrontend.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Services
{
    public interface IServerHealthService
    {
        /// <summary>
        /// 檢查後端是否正常運作
        /// </summary>
        Task<bool> CheckHealthAsync(CancellationToken ct = default);
    }

    public class ServerHealthService : IServerHealthService
    {
        private readonly IHttpService _http;
        public ServerHealthService(IHttpService http) => _http = http;

        /// <summary>
        /// 呼叫後端 /CheckHealth，若回傳 true 代表伺服器健康
        /// </summary>
        public async Task<bool> CheckHealthAsync(CancellationToken ct = default)
        {
            try
            {
                // 假設後端路由為 /CheckHealth
                bool result = await _http.GetJsonAsync<bool>("/Health/CheckHealth", ct);
                return result;
            }
            catch
            {
                // 如果伺服器沒回應或出錯，就回傳 false
                return false;
            }
        }
    }
}
