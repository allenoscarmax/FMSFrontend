using FMSFrontend.Features.Dtos;
using FMSFrontend.Services;
using System.Threading;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Services
{
    public interface IWorksheetsService
    {
        // ==== GET ====
        Task<List<WorksheetsDto>?> GetAllWorkSheetAsync(CancellationToken ct = default);
        Task<WorksheetsDto?> GetWorkSheetByWorkSheetNumberAsync(string workSheetNumber, CancellationToken ct = default);
        Task<List<WorksheetsDto>?> GetWorkSheetByWorkStatusAsync(string workStatus, CancellationToken ct = default);
        Task<List<WorksheetsDto>?> DB_GetWorkSheetsbyWorkpieceName(string workpieceName, CancellationToken ct = default);
        Task<List<WorksheetsDto>?> DB_GetWorkSheetsbyContainWorkpieceName(string workpieceName, CancellationToken ct = default);
        Task<WorksheetsDto?> GetNewestWorkSheetAsync(CancellationToken ct = default);

        // ==== PUT（寫入類，無回傳資料內容）====
        Task<bool> InsertNewWorkSheetDataAsync(WorksheetsDto data, CancellationToken ct = default);
        Task<bool> UpdateWorkSheetDataAsync(WorksheetsDto data, CancellationToken ct = default);
        Task<bool> DeleteAllWorkSheetDataAsync(CancellationToken ct = default);
        Task<bool> DeleteWorkSheetDataByIdAsync(string id, CancellationToken ct = default);

        // ==== 時間軸 / 複合查詢（後端目前是 PUT 但回傳資料 → 建議改 GET）====
        // A. 推薦後端改成 [HttpGet] 後，可直接使用下方這三支：
        Task<List<WorksheetIncludeTimelineDto>?> GetWorkSheetsIncludeTimelineByDateTimeAsync(DateTime start, DateTime end, CancellationToken ct = default);
        Task<WorksheetsTimelineDto?> GetWorksheetsTimelineByWorkSheetSerialAsync(string workSheetSerial, CancellationToken ct = default);
        Task<List<WorksheetsTimelineDto>?> GetWorksheetTimelineByDateTimeAsync(DateTime start, DateTime end, CancellationToken ct = default);

        // B. 若後端暫時無法改 GET，請在 IHttpService 增加 PutReadJsonAsync<T>()，再把上面三支改用 PUT（程式內有註解範例）
    }

    public class WorksheetsService : IWorksheetsService
    {
        private readonly IHttpService _http;
        public WorksheetsService(IHttpService http) => _http = http;

        // ===== GET =====
        public async Task<List<WorksheetsDto>?> GetAllWorkSheetAsync(CancellationToken ct = default)
            => await _http.GetJsonAsync<List<WorksheetsDto>>("Worksheet/DB_GetAllWorkSheet", ct);

        public async Task<WorksheetsDto?> GetWorkSheetByWorkSheetNumberAsync(string workSheetNumber, CancellationToken ct = default)
            => await _http.GetJsonAsync<WorksheetsDto>($"Worksheet/DB_GetWorkSheetByWorkSheetNumber/{Uri.EscapeDataString(workSheetNumber)}", ct);

        public async Task<List<WorksheetsDto>?> GetWorkSheetByWorkStatusAsync(string workStatus, CancellationToken ct = default)
            => await _http.GetJsonAsync<List<WorksheetsDto>>($"Worksheet/DB_GetWorkSheetByWorkStatus/{Uri.EscapeDataString(workStatus)}", ct);

        public async Task<List<WorksheetsDto>?> DB_GetWorkSheetsbyWorkpieceName(string workpieceName, CancellationToken ct = default)
            => await _http.GetJsonAsync<List<WorksheetsDto>>($"Worksheet/DB_GetWorkSheetsbyWorkpieceName/{Uri.EscapeDataString(workpieceName)}", ct);

        public async Task<List<WorksheetsDto>?> DB_GetWorkSheetsbyContainWorkpieceName(string workpieceName, CancellationToken ct = default)
            => await _http.GetJsonAsync<List<WorksheetsDto>>($"Worksheet/DB_GetWorkSheetsbyContainWorkpieceName/{Uri.EscapeDataString(workpieceName)}", ct);

        public async Task<WorksheetsDto?> GetNewestWorkSheetAsync(CancellationToken ct = default)
            => await _http.GetJsonAsync<WorksheetsDto>("Worksheet/DB_GetNewestWorkSheet", ct);

        // ===== PUT（寫入，回傳 bool）=====
        public async Task<bool> InsertNewWorkSheetDataAsync(WorksheetsDto data, CancellationToken ct = default)
            => await _http.SendPutAsync("Worksheet/DB_InsertNewWorkSheetData", data);

        public async Task<bool> UpdateWorkSheetDataAsync(WorksheetsDto data, CancellationToken ct = default)
            => await _http.SendPutAsync("Worksheet/DB_UpdateWorkSheetData", data);

        public async Task<bool> DeleteAllWorkSheetDataAsync(CancellationToken ct = default)
            => await _http.SendPutAsync("Worksheet/DB_DeleteAllWorkSheetData", new { });

        public async Task<bool> DeleteWorkSheetDataByIdAsync(string id, CancellationToken ct = default)
            => await _http.SendPutAsync($"Worksheet/DB_DeleteWorkSheetDataById/{Uri.EscapeDataString(id)}", new { });

        public async Task<List<WorksheetIncludeTimelineDto>?> GetWorkSheetsIncludeTimelineByDateTimeAsync(DateTime start, DateTime end, CancellationToken ct = default)
            => await _http.GetJsonAsync<List<WorksheetIncludeTimelineDto>>(
                $"Worksheet/DB_GetWorkSheetsIncludeTimelineByDateTime/{start:O}/{end:O}", ct);

        public async Task<WorksheetsTimelineDto?> GetWorksheetsTimelineByWorkSheetSerialAsync(string workSheetSerial, CancellationToken ct = default)
            => await _http.GetJsonAsync<WorksheetsTimelineDto>(
                $"Worksheet/DB_GetWorksheetsTimelineByWorkSheetSerial/{Uri.EscapeDataString(workSheetSerial)}", ct);

        public async Task<List<WorksheetsTimelineDto>?> GetWorksheetTimelineByDateTimeAsync(DateTime start, DateTime end, CancellationToken ct = default)
            => await _http.GetJsonAsync<List<WorksheetsTimelineDto>>(
                $"Worksheet/GetWorksheetTimelineByDateTime/{start:O}/{end:O}", ct);
    }
}
