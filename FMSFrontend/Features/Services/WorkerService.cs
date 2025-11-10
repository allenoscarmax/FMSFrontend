using FMSFrontend.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Services
{
    public interface IWorkerService
    {
        // ==== GET ====
        Task<List<WorkerDto>?> GetAllWorkerAsync(CancellationToken ct = default);                     // 取得所有員工
        Task<List<WorkerDto>?> GetWorkerByAccountGroupAsync(string accountGroup, CancellationToken ct = default); // 依群組取得員工

        // ==== PUT ====
        Task<bool> InsertNewWorkerDataAsync(WorkerDto payload, CancellationToken ct = default);       // 新增員工
        Task<bool> UpdateWorkerDataAsync(WorkerDto payload, CancellationToken ct = default);          // 更新員工
        Task<bool> DeleteWorkerDataByIdAsync(string id, CancellationToken ct = default);              // 刪除員工
        Task<bool> DeleteAllWorkerDataAsync(CancellationToken ct = default);                          // 刪除全部員工
    }

    public class WorkerService : IWorkerService
    {
        private readonly IHttpService _http;
        public WorkerService(IHttpService http) => _http = http;

        // ==== GET ====
        public async Task<List<WorkerDto>?> GetAllWorkerAsync(CancellationToken ct = default)
            => await _http.GetJsonAsync<List<WorkerDto>>("Worker/DB_GetAllWorker", ct);

        public async Task<List<WorkerDto>?> GetWorkerByAccountGroupAsync(string accountGroup, CancellationToken ct = default)
            => await _http.GetJsonAsync<List<WorkerDto>>($"Worker/DB_GetWorkerByAccountGroup/{Uri.EscapeDataString(accountGroup)}", ct);

        // ==== PUT ====
        public async Task<bool> InsertNewWorkerDataAsync(WorkerDto payload, CancellationToken ct = default)
            => await _http.SendPutAsync("Worker/DB_InsertNewWorkerData", payload);

        public async Task<bool> UpdateWorkerDataAsync(WorkerDto payload, CancellationToken ct = default)
            => await _http.SendPutAsync("Worker/DB_UpdateWorkerData", payload);

        public async Task<bool> DeleteWorkerDataByIdAsync(string id, CancellationToken ct = default)
            => await _http.SendPutAsync($"Worker/DB_DeleteWorkerDataById/{Uri.EscapeDataString(id)}", new { });

        public async Task<bool> DeleteAllWorkerDataAsync(CancellationToken ct = default)
            => await _http.SendPutAsync("Worker/DB_DeleteAllWorkerData", new { });
    }
}
