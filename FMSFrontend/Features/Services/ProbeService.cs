using FMSFrontend.Features.Dtos;
using FMSFrontend.Services;
using System.Threading;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Services
{
    public interface IProbeService
    {
        // ==== Repository (DB_*) ====
        Task<bool> DB_InsertProbeAsync(ProbeDto payload, CancellationToken ct = default);                 // PUT  Probe/DB_InsertProbe
        Task<bool> DB_UpdateProbeDataAsync(ProbeDto payload, CancellationToken ct = default);             // PUT  Probe/DB_UpdateProbeData

        Task<List<ProbeDto>?> DB_GetAllProbeAsync(CancellationToken ct = default);                        // GET  Probe/DB_GetAllProbe
        Task<ProbeDto?> DB_GetProbeByTagSerialAsync(string tagSerial, CancellationToken ct = default); // GET  Probe/DB_GetProbeByTagSerial/{TagSerial}
        Task<ProbeDto?> DB_GetProbeByIdAsync(string id, CancellationToken ct = default);            // GET  Probe/DB_GetProbeById/{Id}
    }

    public class ProbeService : IProbeService
    {
        private readonly IHttpService _http;
        public ProbeService(IHttpService http) => _http = http;

        private static string Enc(string s) => Uri.EscapeDataString(s ?? string.Empty);

        // ==== Repository (DB_*) ====

        /// <summary>新增探針</summary>
        public async Task<bool> DB_InsertProbeAsync(ProbeDto payload, CancellationToken ct = default)
            => await _http.SendPutAsync("Probe/DB_InsertProbe", payload);

        /// <summary>更新探針</summary>
        public async Task<bool> DB_UpdateProbeDataAsync(ProbeDto payload, CancellationToken ct = default)
            => await _http.SendPutAsync("Probe/DB_UpdateProbeData", payload);

        /// <summary>取得全部探針</summary>
        public async Task<List<ProbeDto>?> DB_GetAllProbeAsync(CancellationToken ct = default)
            => await _http.GetJsonAsync<List<ProbeDto>>("Probe/DB_GetAllProbe", ct);

        /// <summary>以 TagSerial 取得單筆探針</summary>
        public async Task<ProbeDto?> DB_GetProbeByTagSerialAsync(string tagSerial, CancellationToken ct = default)
        { 
           var json =  await _http.GetJsonAsync<ProbeDto>($"Probe/DB_GetProbeByTagSerial/{Enc(tagSerial)}", ct);
            return json;
        }

        /// <summary>以 Id 取得單筆探針</summary>
        public async Task<ProbeDto?> DB_GetProbeByIdAsync(string id, CancellationToken ct = default)
            => await _http.GetJsonAsync<ProbeDto>($"Probe/DB_GetProbeById/{Enc(id)}", ct);
    }
}
