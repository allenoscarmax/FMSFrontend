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
        Task<AsrsParameterDto?> GetAsrsParameterAsync(CancellationToken ct = default);
        Task<List<RobotDto>?> DB_GetAllRobotsAsync(CancellationToken ct = default);
        Task<bool> SetRobotStartAsync();
        Task<bool> SetRobotPauseAsync();
        Task<bool> SetRobotStopAsync();
        Task<bool> SetASRSDispatchSwitchAsync(bool enabled, CancellationToken ct = default);
        Task<bool> ASRSRobotResetStatus(int no, CancellationToken ct = default);

    }

    public class RobotService : IRobotService
    {
        private readonly IHttpService _http;
        public RobotService(IHttpService http) => _http = http;

        public async Task<AsrsParameterDto?> GetAsrsParameterAsync(CancellationToken ct = default)
        {
            // TODO: 改成你的實際路由
            return await _http.GetJsonAsync<AsrsParameterDto>("ASRS/GetASRSParameter", ct);
        }
        public async Task<List<RobotDto>?> DB_GetAllRobotsAsync(CancellationToken ct = default)
        {
            return await _http.GetJsonAsync<List<RobotDto>>("Robot/DB_GetAllRobots", ct)
                   ?? new List<RobotDto>();
        }
        public async Task<bool> SetRobotStartAsync()
        {
            const string route = "ASRS/SetASRSRobotStart";
            return await _http.SendPutAsync(route, new { });
        }

        public async Task<bool> SetRobotPauseAsync()
        {
            const string route = "ASRS/SetASRSRobotPause";
            return await _http.SendPutAsync(route, new { });
        }

        public async Task<bool> SetRobotStopAsync()
        {
            const string route = "ASRS/SetASRSRobotStop";
            return await _http.SendPutAsync(route, new { });
        }
        public async Task<bool> SetASRSDispatchSwitchAsync(bool enabled, CancellationToken ct = default)
        {
            var route = $"ASRS/SetASRSDispatchSwitch/{enabled.ToString().ToLower()}";
            return await _http.SendPutAsync(route, new { });
        }
        public async Task<bool> ASRSRobotResetStatus(int no, CancellationToken ct = default)
        {
            var route = $"ASRS/ASRSRobotResetStatus/{no.ToString().ToLower()}";
            return await _http.SendPutAsync(route, new { });
        }
    }
}
