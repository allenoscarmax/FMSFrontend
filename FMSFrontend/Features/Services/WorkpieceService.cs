using FMSFrontend.Features.Dtos;
using FMSFrontend.Services;
using System.Threading;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Services
{
    public interface IWorkpieceService
    {
        Task<List<WorkpieceDto>?> GetAllWorkpieceAsync(CancellationToken ct = default);
        Task<bool> UpdateWorkpieceDataAsync(WorkpieceDto payload, CancellationToken ct = default);
    }

    public class WorkpieceService : IWorkpieceService
    {
        private readonly IHttpService _http;
        public WorkpieceService(IHttpService http) => _http = http;
        //====GET====

        //取得所有工件資料
        public async Task<List<WorkpieceDto>?> GetAllWorkpieceAsync(CancellationToken ct = default)
        {
            return await _http.GetJsonAsync<List<WorkpieceDto>>("Workpiece/DB_GetAllWorkpiece", ct);
        }

        //====PUT====

        //更新工件資料
        public async Task<bool> UpdateWorkpieceDataAsync(WorkpieceDto payload, CancellationToken ct = default)
        {
            return await _http.SendPutAsync($"Workpiece/DB_UpdateWorkpieceData", payload);
        }
    }
}
