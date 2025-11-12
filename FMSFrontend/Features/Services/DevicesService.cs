using FMSFrontend.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FMSFrontend.Features.Dtos;

namespace FMSFrontend.Features.Services
{
    public interface IDevicesService
    {
        // ==== Repository (DB_*) ====
        Task<List<DevicesDto>?> GetAllDevicesAsync(CancellationToken ct = default);          // GET  Device/DB_GetAllDevices
        Task<bool> InsertNewDevicesDataAsync(DevicesDto payload, CancellationToken ct = default); // PUT  Device/DB_InsertNewDevicesData
        Task<bool> UpdateDeviceDataAsync(DevicesDto payload, CancellationToken ct = default);     // PUT  Device/DB_UpdateDeviceData
        Task<bool> DeleteDevicesDatabyIdAsync(string id, CancellationToken ct = default);        // PUT  Device/DB_DeleteDevicesDatabyId/{id}
    }

    public class DevicesService : IDevicesService
    {
        private readonly IHttpService _http;
        public DevicesService(IHttpService http) => _http = http;

        /// <summary>取得所有設備</summary>
        public async Task<List<DevicesDto>?> GetAllDevicesAsync(CancellationToken ct = default)
            => await _http.GetJsonAsync<List<DevicesDto>>("Device/DB_GetAllDevices", ct);

        /// <summary>新增設備資料</summary>
        public async Task<bool> InsertNewDevicesDataAsync(DevicesDto payload, CancellationToken ct = default)
            => await _http.SendPutAsync("Device/DB_InsertNewDevicesData", payload);

        /// <summary>更新設備資料</summary>
        public async Task<bool> UpdateDeviceDataAsync(DevicesDto payload, CancellationToken ct = default)
            => await _http.SendPutAsync("Device/DB_UpdateDeviceData", payload);

        /// <summary>依 Id 刪除設備資料</summary>
        public async Task<bool> DeleteDevicesDatabyIdAsync(string id, CancellationToken ct = default)
            => await _http.SendPutAsync($"Device/DB_DeleteDevicesDatabyId/{Uri.EscapeDataString(id)}", new { });
    }
}
