using FMSFrontend.Features.Dtos;
using FMSFrontend.Services;
using System.Threading;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Services
{
    public interface IPlcService
    {
        Task<MagazineParaDto?> GetALLMagazineParaAsync(CancellationToken ct = default); // 取得倉狀態
        Task<bool> EleMagzineDoorLightSwitchAsync(int doorNum, bool lightOn);   // 開關倉門燈
        Task<bool> EleMagzineDoorSwitchAsync(int doorNum, int upOrDown, bool open); // 開關倉門（doorNum: 門號, upOrDown: 0=上門, 1=下門, open: 開/關）
    }

    public class PlcService : IPlcService
    {
        private readonly IHttpService _http;
        public PlcService(IHttpService http) => _http = http;

        public async Task<MagazineParaDto?> GetALLMagazineParaAsync(CancellationToken ct = default)
        {
            return await _http.GetJsonAsync<MagazineParaDto>("PLC/GetALLMagzinePara", ct);
        }

        public async Task<bool> EleMagzineDoorLightSwitchAsync(int doorNum, bool lightOn)
        {
            var route = $"PLC/EleMagzineDoorLightSwitch/{doorNum}/{lightOn.ToString().ToLower()}";
            return await _http.SendPutAsync(route, new { });
        }

        public async Task<bool> EleMagzineDoorSwitchAsync(int doorNum, int upOrDown, bool open)
        {
            var route = $"PLC/ELEMagzineDoorSwitch/{doorNum}/{upOrDown}/{open.ToString().ToLower()}";
            return await _http.SendPutAsync(route, new { });
        }
    }
}
