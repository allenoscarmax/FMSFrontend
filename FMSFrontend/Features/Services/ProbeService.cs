using FMSFrontend.Features.Dtos;
using FMSFrontend.Services;
using System.Threading;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Services
{
    public interface IProbeService
    {
        Task<List<ProbeDto>?> GetAllProbeAsync(CancellationToken ct = default); //取得所有探針資料
        Task<bool> UpdateProbeDataAsync(ProbeDto payload, CancellationToken ct = default); //更新探針資料
    }

    public class ProbeService : IProbeService
    {
        private readonly IHttpService _http;
        public ProbeService(IHttpService http) => _http = http;
        //====GET====
        public async Task<List<ProbeDto>?> GetAllProbeAsync(CancellationToken ct = default)
        {
            return await _http.GetJsonAsync<List<ProbeDto>>("Probe/DB_GetAllProbe", ct);
        }
        //====PUT====
        public async Task<bool> UpdateProbeDataAsync(ProbeDto payload, CancellationToken ct = default)
        {
            return await _http.SendPutAsync($"Probe/DB_UpdateProbeData", payload);
        }
    }
}
