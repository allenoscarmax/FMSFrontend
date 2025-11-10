using FMSFrontend.Features.Dtos;
using FMSFrontend.Services;
using System.Threading;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Services
{
    public interface IPlcService
    {
        // ===== Read (GET) =====
        Task<MagazineParaDto?> GetALLMagazineParaAsync(CancellationToken ct = default); // GET  PLC/GetALLMagzinePara
        Task<AssemblyStationParaDto?> GetAssemblyStationParaAsync(CancellationToken ct = default); // GET PLC/GetAssemblyStationPara
        Task<PLCCommonParaDto?> GetPLCCommonParaAsync(CancellationToken ct = default);    // GET  PLC/GetPLCCommonPara

        // ===== Actions (PUT) =====
        Task<bool> EleMagzineDoorSwitchAsync(int doorNum, int upOrDown, bool open);              // PUT  PLC/ELEMagzineDoorSwitch/{DoorNum}/{UpandDown}/{Open}
        Task<bool> EleMagzineDoorLightSwitchAsync(int doorNum, bool lightOn);                    // PUT  PLC/EleMagzineDoorLightSwitch/{DoorNum}/{LightSwitch}

        Task<bool> ASE_OpenDoorAsync();                                                          // PUT  PLC/ASE_OpenDoor
        Task<bool> ASE_OpenDoorLightAsync(bool open);                                            // PUT  PLC/ASE_OpenDoorLight/{Open}
        Task<bool> ASE_OpenChuckAsync(bool open);                                                // PUT  PLC/ASE_OpenChuck/{Open}
        Task<bool> ASE_Require_OutcomingPartAsync(bool open);                                    // PUT  PLC/ASE_Require_OutcomingPart/{Open}
        Task<bool> ASE_Require_IncomingPartAsync(bool open);                                     // PUT  PLC/ASE_Require_IncomingPart/{Open}

        Task<bool> LubricatorStartAsync();                                                       // PUT  PLC/LubricatorStart
        Task<bool> BalluffPowerAsync(bool open);                                                 // PUT  PLC/BalluffPower/{Open}
    }

    public class PlcService : IPlcService
    {
        private readonly IHttpService _http;
        public PlcService(IHttpService http) => _http = http;

        // ===== Read (GET) =====
        public async Task<MagazineParaDto?> GetALLMagazineParaAsync(CancellationToken ct = default)
            => await _http.GetJsonAsync<MagazineParaDto>("PLC/GetALLMagzinePara", ct);

        public async Task<AssemblyStationParaDto?> GetAssemblyStationParaAsync(CancellationToken ct = default)
            => await _http.GetJsonAsync<AssemblyStationParaDto>("PLC/GetAssemblyStationPara", ct);

        public async Task<PLCCommonParaDto?> GetPLCCommonParaAsync(CancellationToken ct = default)
            => await _http.GetJsonAsync<PLCCommonParaDto>("PLC/GetPLCCommonPara", ct);

        // ===== Actions (PUT) =====
        public async Task<bool> EleMagzineDoorLightSwitchAsync(int doorNum, bool lightOn)
        {
            var route = $"PLC/EleMagzineDoorLightSwitch/{doorNum}/{BoolSeg(lightOn)}";
            return await _http.SendPutAsync(route, new { });
        }

        public async Task<bool> EleMagzineDoorSwitchAsync(int doorNum, int upOrDown, bool open)
        {
            var route = $"PLC/ELEMagzineDoorSwitch/{doorNum}/{upOrDown}/{BoolSeg(open)}";
            return await _http.SendPutAsync(route, new { });
        }

        public async Task<bool> ASE_OpenDoorAsync()
            => await _http.SendPutAsync("PLC/ASE_OpenDoor", new { });

        public async Task<bool> ASE_OpenDoorLightAsync(bool open)
            => await _http.SendPutAsync($"PLC/ASE_OpenDoorLight/{BoolSeg(open)}", new { });

        public async Task<bool> ASE_OpenChuckAsync(bool open)
            => await _http.SendPutAsync($"PLC/ASE_OpenChuck/{BoolSeg(open)}", new { });

        public async Task<bool> ASE_Require_OutcomingPartAsync(bool open)
            => await _http.SendPutAsync($"PLC/ASE_Require_OutcomingPart/{BoolSeg(open)}", new { });

        public async Task<bool> ASE_Require_IncomingPartAsync(bool open)
            => await _http.SendPutAsync($"PLC/ASE_Require_IncomingPart/{BoolSeg(open)}", new { });

        public async Task<bool> LubricatorStartAsync()
            => await _http.SendPutAsync("PLC/LubricatorStart", new { });

        public async Task<bool> BalluffPowerAsync(bool open)
            => await _http.SendPutAsync($"PLC/BalluffPower/{BoolSeg(open)}", new { });

        // helper：將 bool 轉為小寫路由段
        private static string BoolSeg(bool v) => v ? "true" : "false";
    }
}
