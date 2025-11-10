using FMSFrontend.Features.Dtos;
using FMSFrontend.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Services
{
    public interface IRobotService
    {
        // ===== ASRS 狀態參數（沿用你既有的）=====
        Task<AsrsParameterDto?> GetAsrsParameterAsync(CancellationToken ct = default);

        // ===== 機器人 Repository =====
        Task<List<RobotDto>?> DB_GetAllRobotsAsync(CancellationToken ct = default);
        Task<bool> DB_UpdateRobotDataAsync(RobotDto robot, CancellationToken ct = default);
        Task<bool> DB_SetRobotStatusByIdAsync(string id, string status, CancellationToken ct = default);          // PUT Robot/DB_SetRobotStatusbyId/{Id}?Status=...
        Task<bool> DB_SetRobotOnDeckObjSerialByIdAsync(string id, string onDeckObjSerial, CancellationToken ct = default); // PUT Robot/DB_SetRobotOnDeckObjSerialbyId/{Id}?OnDeckObjSerial=...
        Task<bool> DB_SetRobotReleaseObjByIdAsync(string id, CancellationToken ct = default);                     // PUT Robot/DB_SetRobotReleaseObjbyId/{Id}

        // ===== 機器人 Function =====
        Task<string?> GetRobotIPAsync(int no, CancellationToken ct = default);                // GET Robot/GetRobotIP/{No}（取 AdditionalInfo 當 IP）
        Task<bool> ASRSRobotResetStatusAsync(int no, CancellationToken ct = default);         // PUT Robot/ASRSRobotResetStatus/{No}

        // ===== ASRS 控制（你原本就有的）=====
        Task<bool> SetRobotStartAsync();
        Task<bool> SetRobotPauseAsync();
        Task<bool> SetRobotStopAsync();
        Task<bool> SetASRSDispatchSwitchAsync(bool enabled, CancellationToken ct = default);
    }

    public class RobotService : IRobotService
    {
        private readonly IHttpService _http;
        public RobotService(IHttpService http) => _http = http;

        // ===== ASRS 參數 =====
        public async Task<AsrsParameterDto?> GetAsrsParameterAsync(CancellationToken ct = default)
            => await _http.GetJsonAsync<AsrsParameterDto>("ASRS/GetASRSParameter", ct);

        // ===== Repository =====
        public async Task<List<RobotDto>?> DB_GetAllRobotsAsync(CancellationToken ct = default)
            => await _http.GetJsonAsync<List<RobotDto>>("Robot/DB_GetAllRobots", ct) ?? new List<RobotDto>();

        public async Task<bool> DB_UpdateRobotDataAsync(RobotDto robot, CancellationToken ct = default)
            => await _http.SendPutAsync("Robot/DB_UpdateRobotData", robot);

        public async Task<bool> DB_SetRobotStatusByIdAsync(string id, string status, CancellationToken ct = default)
            => await _http.SendPutAsync(
                $"Robot/DB_SetRobotStatusbyId/{Uri.EscapeDataString(id)}?Status={Uri.EscapeDataString(status)}",
                new { });

        public async Task<bool> DB_SetRobotOnDeckObjSerialByIdAsync(string id, string onDeckObjSerial, CancellationToken ct = default)
            => await _http.SendPutAsync(
                $"Robot/DB_SetRobotOnDeckObjSerialbyId/{Uri.EscapeDataString(id)}?OnDeckObjSerial={Uri.EscapeDataString(onDeckObjSerial)}",
                new { });

        public async Task<bool> DB_SetRobotReleaseObjByIdAsync(string id, CancellationToken ct = default)
            => await _http.SendPutAsync(
                $"Robot/DB_SetRobotReleaseObjbyId/{Uri.EscapeDataString(id)}",
                new { });

        // ===== Function =====

        public async Task<string?> GetRobotIPAsync(int no, CancellationToken ct = default)
        {
            var res = await _http.GetJsonAsync<string>($"Robot/GetRobotIP/{no}", ct);
            return res; // 後端 { Message, AdditionalInfo }，這裡直接取 IP
        }

        public async Task<bool> ASRSRobotResetStatusAsync(int no, CancellationToken ct = default)
            => await _http.SendPutAsync($"Robot/ASRSRobotResetStatus/{no}", new { });

        // ===== ASRS 控制 =====
        public async Task<bool> SetRobotStartAsync()
            => await _http.SendPutAsync("ASRS/SetASRSRobotStart", new { });

        public async Task<bool> SetRobotPauseAsync()
            => await _http.SendPutAsync("ASRS/SetASRSRobotPause", new { });

        public async Task<bool> SetRobotStopAsync()
            => await _http.SendPutAsync("ASRS/SetASRSRobotStop", new { });

        public async Task<bool> SetASRSDispatchSwitchAsync(bool enabled, CancellationToken ct = default)
            => await _http.SendPutAsync($"ASRS/SetASRSDispatchSwitch/{(enabled ? "true" : "false")}", new { });
    }
}
