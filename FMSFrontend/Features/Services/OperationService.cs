using ControlzEx.Standard;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Services
{
    public interface IOperationMessageLogService
    {
        // ==== GET ====
        Task<List<OperationMessageLogDto>?> GetAllOperationMessageLogAsync(CancellationToken ct = default);
        Task<List<OperationMessageLogDto>?> GetOperationMessageLogByDateAsync(DateTime startTime, DateTime endTime, CancellationToken ct = default);

        // ==== PUT ====
        Task<bool> InsertNewOperationMessageLogDataAsync(OperationMessageLogDto payload, CancellationToken ct = default);
        Task<bool> DeleteOperationMessageLogDataByIdAsync(string id, CancellationToken ct = default);
        Task<bool> DeleteAllOperationMessageLogDataAsync(CancellationToken ct = default);
    }

    public class OperationMessageLogService : IOperationMessageLogService
    {
        private readonly IHttpService _http;
        public OperationMessageLogService(IHttpService http) => _http = http;

        // ==== GET ====
        public async Task<List<OperationMessageLogDto>?> GetAllOperationMessageLogAsync(CancellationToken ct = default)
            => await _http.GetJsonAsync<List<OperationMessageLogDto>>("OperationLog/DB_GetAllOperationMessageLog", ct);

        public async Task<List<OperationMessageLogDto>?> GetOperationMessageLogByDateAsync(DateTime startTime, DateTime endTime, CancellationToken ct = default)
        {
            // 跟你後台路由一致：DB_GetOperationMessageLogByDate/{StartTime}/{EndTime}
            // 用 "O" (round-trip) 避免文化/格式問題，並 Escape
            string s = Uri.EscapeDataString(startTime.ToString("O"));
            string e = Uri.EscapeDataString(endTime.ToString("O"));
            return await _http.GetJsonAsync<List<OperationMessageLogDto>>($"OperationLog/DB_GetOperationMessageLogByDate/{s}/{e}", ct);
        }

        // ==== PUT ====
        public async Task<bool> InsertNewOperationMessageLogDataAsync(OperationMessageLogDto payload, CancellationToken ct = default)
            => await _http.SendPutAsync("OperationLog/DB_InsertNewOperationMessageLogData", payload);

        public async Task<bool> DeleteOperationMessageLogDataByIdAsync(string id, CancellationToken ct = default)
            => await _http.SendPutAsync($"OperationLog/DB_DeleteOperationMessageLogDataById/{Uri.EscapeDataString(id)}", new { });

        public async Task<bool> DeleteAllOperationMessageLogDataAsync(CancellationToken ct = default)
            => await _http.SendPutAsync("OperationLog/DB_DeleteAllOperationMessageLogData", new { });
    }
}
