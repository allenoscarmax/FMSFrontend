using FMSFrontend.Features.Dtos;
using FMSFrontend.Services;
using System.Threading;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Services
{
    public interface IWorkpieceService
    {
        //====GET====
        Task<List<WorkpieceDto>?> GetAllWorkpieceAsync(CancellationToken ct = default);//取得所有工件資料
        Task<List<WorkpieceDto>?> GetWorkpieceByTagSerialAsync(string TagSerial, CancellationToken ct = default);  //由序號取得工件資料
        Task<List<WpTimelineDto>?> GetWorkpieceTimelineAsync(string id, CancellationToken ct = default); //由ID取得工件加工進度
        //====PUT====
        Task<bool> UpdateWorkpieceDataAsync(WorkpieceDto payload, CancellationToken ct = default);
    }

    public class WorkpieceService : IWorkpieceService
    {
        private readonly IHttpService _http;
        public WorkpieceService(IHttpService http) => _http = http;
        //====GET====
        public async Task<List<WorkpieceDto>?> GetAllWorkpieceAsync(CancellationToken ct = default)  //取得所有工件資料
        {
            return await _http.GetJsonAsync<List<WorkpieceDto>>("Workpiece/DB_GetAllWorkpiece", ct);
        }
        public async Task<List<WorkpieceDto>?> GetWorkpieceByTagSerialAsync(string TagSerial, CancellationToken ct = default)  //由序號取得工件資料
        {
            return await _http.GetJsonAsync<List<WorkpieceDto>>($"Workpiece/DB_GetWorkpieceByTagSerial/{TagSerial}", ct);
        }
        public async Task<List<WpTimelineDto>?> GetWorkpieceTimelineAsync(string id, CancellationToken ct = default) //由ID取得工件加工進度
        {
            return await _http.GetJsonAsync<List<WpTimelineDto>>($"DB_GetWorkpieceTimelineByWorkpieceId/{id}", ct);
        }
        //====PUT====
        public async Task<bool> UpdateWorkpieceDataAsync(WorkpieceDto payload, CancellationToken ct = default) //更新工件資料
        {
            return await _http.SendPutAsync($"Workpiece/DB_UpdateWorkpieceData", payload);
        }
    }
}
