using FMSFrontend.Features.Dtos;
using FMSFrontend.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Services
{
    public interface IWorkpieceService
    {
        Task<List<WorkpieceDto>?> GetAllWorkpieceAsync(CancellationToken ct = default); // 取得所有工件
        Task<WorkpieceDto?> GetWorkpieceByTagSerialAsync(string tagSerial, CancellationToken ct = default);       // 序號篩選
        Task<WorkpieceDto?> GetWorkpieceByWorksheetNumberAsync(string WorksheetNumber, CancellationToken ct = default); //由工單篩選
        Task<List<WpTimelineDto>?> GetWorkpieceTimelineByWorkpieceIdAsync(string workpieceId, CancellationToken ct = default); //由Id取得工件 Timeline

        Task<bool> UpdateWorkpieceDataAsync(WorkpieceDto payload, CancellationToken ct = default); //更新工件
        Task<bool> SetWorkpieceRestrictionByTagSerialAsync(string tagSerial, bool restriction, CancellationToken ct = default); //用序號禁用工件
        Task<bool> DeleteWorkpieceDataByIdAsync(string id, CancellationToken ct = default); //用ID刪除工件

        //Task<List<WorkpieceDto>?> GetWorkpiecesByTagSerialAsync(string tagSerial, CancellationToken ct = default); // 多筆（另一支API）
        //Task<List<WorkpieceDto>?> GetAllOnShelfWorkpieceAsync(CancellationToken ct = default);
        //Task<WorkpieceDto?> GetWorkpieceByIdAsync(string id, CancellationToken ct = default);
        // Timeline
        //Task<List<WpTimelineDto>?> GetWorkpieceTimelineByIdAsync(string id, CancellationToken ct = default);                    // PUT（空 body）
        //Task<List<WpTimelineDto>?> GetWorkpieceTimelineByDateAsync(DateTime start, DateTime end, CancellationToken ct = default);

        // ==== PUT ====
        //Task<bool> InsertWorkpieceAsync(WorkpieceDto payload, CancellationToken ct = default);
        //Task<bool> SetWorkpieceRestrictionByIdAsync(string id, bool restriction, CancellationToken ct = default);
        //Task<bool> SetWorkpieceStatusByIdAsync(string id, string status, CancellationToken ct = default);
        //Task<bool> SetWorkpieceCurrentLocationByIdAsync(string id, string currentLocation, CancellationToken ct = default);
        //Task<bool> DeleteAllWorkpieceDataAsync(CancellationToken ct = default);
        //Task<bool> RemoveWorkpieceTagSerialDataByIdAsync(string id, CancellationToken ct = default);
        //Task<bool> DeleteAllWorkpieceTimelineAsync(CancellationToken ct = default);
    }

    public class WorkpieceService : IWorkpieceService
    {
        private readonly IHttpService _http;
        public WorkpieceService(IHttpService http) => _http = http;

        private static string Enc(string s) => Uri.EscapeDataString(s ?? string.Empty);

        // ==== GET ====
        public Task<List<WorkpieceDto>?> GetAllWorkpieceAsync(CancellationToken ct = default)
            => _http.GetJsonAsync<List<WorkpieceDto>>("Workpiece/DB_GetAllWorkpiece", ct);

        public Task<List<WorkpieceDto>?> GetAllOnShelfWorkpieceAsync(CancellationToken ct = default)
            => _http.GetJsonAsync<List<WorkpieceDto>>("Workpiece/DB_GetAllOnShelfWorkpiece", ct);

        public Task<WorkpieceDto?> GetWorkpieceByIdAsync(string id, CancellationToken ct = default)
            => _http.GetJsonAsync<WorkpieceDto>($"Workpiece/DB_GetWorkpieceById/{Enc(id)}", ct);

        // 單筆（依後台 DB_GetWorkpieceByTagSerial 回傳單筆）
        public Task<WorkpieceDto?> GetWorkpieceByTagSerialAsync(string tagSerial, CancellationToken ct = default)
            => _http.GetJsonAsync<WorkpieceDto>($"Workpiece/DB_GetWorkpieceByTagSerial/{Enc(tagSerial)}", ct);

        // 多筆（另一支 API：DB_GetWorkpiecesbyTagSerial）
        public Task<List<WorkpieceDto>?> GetWorkpiecesByTagSerialAsync(string tagSerial, CancellationToken ct = default)
            => _http.GetJsonAsync<List<WorkpieceDto>>($"Workpiece/DB_GetWorkpiecesbyTagSerial/{Enc(tagSerial)}", ct);
        public Task<WorkpieceDto?> GetWorkpieceByWorksheetNumberAsync(string WorksheetNumber, CancellationToken ct = default)
            => _http.GetJsonAsync<WorkpieceDto>($"Workpiece/DB_GetWorkpieceByWorksheetNumber/{Enc(WorksheetNumber)}", ct);

        public Task<WorkpieceDto?> GetWorkpieceByWorksheetNumber(string WorksheetNumber, CancellationToken ct = default)
            => _http.GetJsonAsync<WorkpieceDto>($"Workpiece/DB_GetWorkpieceByWorksheetNumber/{Enc(WorksheetNumber)}", ct);

        // Timeline（GET）
        public Task<List<WpTimelineDto>?> GetWorkpieceTimelineByWorkpieceIdAsync(string workpieceId, CancellationToken ct = default)
            => _http.GetJsonAsync<List<WpTimelineDto>>($"Workpiece/DB_GetWorkpieceTimelineByWorkpieceId/{Enc(workpieceId)}", ct);

        // Timeline（PUT，空 body）
        public Task<List<WpTimelineDto>?> GetWorkpieceTimelineByIdAsync(string id, CancellationToken ct = default)
            => _http.GetJsonAsync<List<WpTimelineDto>>($"Workpiece/DB_GetWorkpieceTimelineById/{Enc(id)}", ct);
        // 若你想完全依後端是 PUT：也可改成：
        // => _http.SendPutAsync($"Workpiece/DB_GetWorkpieceTimelineById/{Enc(id)}", new { });

        public Task<List<WpTimelineDto>?> GetWorkpieceTimelineByDateAsync(DateTime start, DateTime end, CancellationToken ct = default)
            => _http.GetJsonAsync<List<WpTimelineDto>>($"Workpiece/DB_GetWorkpieceTimelineByDateTime/{start:O}/{end:O}", ct);




        // ==== PUT ====
        public Task<bool> InsertWorkpieceAsync(WorkpieceDto payload, CancellationToken ct = default)
            => _http.SendPutAsync("Workpiece/DB_InsertWorkpiece", payload);

        public Task<bool> UpdateWorkpieceDataAsync(WorkpieceDto payload, CancellationToken ct = default)
            => _http.SendPutAsync("Workpiece/DB_UpdateWorkpieceData", payload);

        public Task<bool> SetWorkpieceRestrictionByTagSerialAsync(string tagSerial, bool restriction, CancellationToken ct = default)
            => _http.SendPutAsync($"Workpiece/DB_SetWorkpieceRestrictionbyTagSerial/{Enc(tagSerial)}/{restriction.ToString().ToLower()}", new { });

        public Task<bool> SetWorkpieceRestrictionByIdAsync(string id, bool restriction, CancellationToken ct = default)
            => _http.SendPutAsync($"Workpiece/DB_SetWorkpieceRestrictionbyId/{Enc(id)}/{restriction.ToString().ToLower()}", new { });

        public Task<bool> SetWorkpieceStatusByIdAsync(string id, string status, CancellationToken ct = default) 
            => _http.SendPutAsync($"Workpiece/DB_SetWorkpieceStatusbyId/{Enc(id)}/{Enc(status)}", new { });

        public Task<bool> SetWorkpieceCurrentLocationByIdAsync(string id, string currentLocation, CancellationToken ct = default)
            => _http.SendPutAsync($"Workpiece/DB_SetWorkpieceCurrentLocationbyId/{Enc(id)}/{Enc(currentLocation)}", new { });

        public Task<bool> DeleteAllWorkpieceDataAsync(CancellationToken ct = default)
            => _http.SendPutAsync("Workpiece/DB_DeleteAllWorkpieceData", new { });

        public Task<bool> DeleteWorkpieceDataByIdAsync(string id, CancellationToken ct = default)
            => _http.SendPutAsync($"Workpiece/DB_DeleteWorkpieceDataById/{Enc(id)}", new { });

        public Task<bool> RemoveWorkpieceTagSerialDataByIdAsync(string id, CancellationToken ct = default)
            => _http.SendPutAsync($"Workpiece/DB_RemoveWorkpieceTagSerialDatabyId/{Enc(id)}", new { });

        public Task<bool> DeleteAllWorkpieceTimelineAsync(CancellationToken ct = default)
            => _http.SendPutAsync("Workpiece/DB_DeleteAllWorkpieceTimeline", new { });
    }
}
