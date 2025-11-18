using FMSFrontend.Features.Dtos.Database;
using FMSFrontend.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Services
{
    public interface IAlarmService
    {
        Task<List<ErrorMessageLogDto>?> GetCurrentErrorMessageLogAsync(
            CancellationToken ct = default);

        Task<List<ErrorMessageLogDto>?> GetErrorMessageLogByDateTimeAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default);

        Task<bool> RemoveErrorMessageLogByDateTimeAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default);

        Task<bool> DeleteAllErrorMessageDataAsync(
            CancellationToken ct = default);

    }
    public class AlarmService : IAlarmService
    {
        private readonly IHttpService _http;
        public AlarmService(IHttpService http) => _http = http;

        // 目前 ErrorMessage
        public async Task<List<ErrorMessageLogDto>?> GetCurrentErrorMessageLogAsync(
            CancellationToken ct = default)
        {
            return await _http.GetJsonAsync<List<ErrorMessageLogDto>>(
                "Alarm/DB_GetCurrentErrorMessageLog", ct);
        }

        // 依時間區間取 ErrorMessage
        public async Task<List<ErrorMessageLogDto>?> GetErrorMessageLogByDateTimeAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default)
        {
            // 用 ISO 8601 格式帶到 URL
            var start = Uri.EscapeDataString(startDate.ToString("O"));
            var end = Uri.EscapeDataString(endDate.ToString("O"));

            var route = $"Alarm/DB_GetErrorMessageLogByDateTime/{start}/{end}";
            return await _http.GetJsonAsync<List<ErrorMessageLogDto>>(route, ct);
        }

        // 刪除某時間區間內的 ErrorMessage
        public async Task<bool> RemoveErrorMessageLogByDateTimeAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default)
        {
            var start = Uri.EscapeDataString(startDate.ToString("O"));
            var end = Uri.EscapeDataString(endDate.ToString("O"));

            var route = $"Alarm/DB_RemoveErrorMessageLogByDateTime/{start}/{end}";
            return await _http.SendPutAsync(route, new { });
        }

        // 刪除全部 ErrorMessage
        public async Task<bool> DeleteAllErrorMessageDataAsync(
            CancellationToken ct = default)
        {
            const string route = "Alarm/DB_DeleteAllErrorMessageData";
            return await _http.SendPutAsync(route, new { });
        }

    }
}
