using FMSFrontend.Features.Dtos;
using FMSFrontend.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Services
{
    public interface IAppointmentMaintenanceService
    {
        // ==== GET ====
        Task<List<AppointmentMaintenanceDto>?> GetAllAppointmentMaintenanceAsync(CancellationToken ct = default);

        // ==== PUT ====
        Task<bool> InsertAppointmentMaintenanceAsync(AppointmentMaintenanceDto payload, CancellationToken ct = default);
        Task<bool> UpdateAppointmentMaintenanceAsync(AppointmentMaintenanceDto payload, CancellationToken ct = default);
        Task<bool> DeleteAppointmentMaintenanceByIdAsync(string id, CancellationToken ct = default);
    }
    public class AppointmentMaintenanceService : IAppointmentMaintenanceService
    {
        private readonly IHttpService _http;
        public AppointmentMaintenanceService(IHttpService http) => _http = http;

        // ==== GET ====
        public async Task<List<AppointmentMaintenanceDto>?> GetAllAppointmentMaintenanceAsync(CancellationToken ct = default)
            => await _http.GetJsonAsync<List<AppointmentMaintenanceDto>>("AppointmentMaintenance/DB_GetAllAppointmentMaintenance", ct);

        // ==== PUT ====
        public async Task<bool> InsertAppointmentMaintenanceAsync(AppointmentMaintenanceDto payload, CancellationToken ct = default)
            => await _http.SendPutAsync("AppointmentMaintenance/DB_InsertAppointmentMaintenance", payload);

        public async Task<bool> UpdateAppointmentMaintenanceAsync(AppointmentMaintenanceDto payload, CancellationToken ct = default)
            => await _http.SendPutAsync("AppointmentMaintenance/DB_UpdateAppointmentMaintenance", payload);

        public async Task<bool> DeleteAppointmentMaintenanceByIdAsync(string id, CancellationToken ct = default)
            => await _http.SendPutAsync($"AppointmentMaintenance/DB_DeleteAppointmentMaintenanceDataById/{Uri.EscapeDataString(id)}", new { });
    }
}
