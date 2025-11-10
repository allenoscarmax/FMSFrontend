using FMSFrontend.Features.Dtos;
using FMSFrontend.Services;
using System.Threading;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Services
{
    public interface IWorksheetsService
    {
        public Task<List<WorksheetsDto>?> GetAllWorkSheetAsync(CancellationToken ct = default);
        public Task<List<WorksheetsDto>?> DB_GetWorkSheetsbyWorkpieceName(string WorkpieceName, CancellationToken ct = default);
        public Task<List<WorksheetsDto>?> DB_GetWorkSheetsbyContainWorkpieceName(string WorkpieceName, CancellationToken ct = default);
    }

    public class WorksheetsService : IWorksheetsService
    {
        private readonly IHttpService _http;
        public WorksheetsService(IHttpService http) => _http = http;
        //====GET====
        public async Task<List<WorksheetsDto>?> GetAllWorkSheetAsync(CancellationToken ct = default)
        {
            return await _http.GetJsonAsync<List<WorksheetsDto>>("Worksheet/DB_GetAllWorkSheet", ct);
        }
        public async Task<List<WorksheetsDto>?> DB_GetWorkSheetsbyWorkpieceName(string WorkpieceName, CancellationToken ct = default)
        {
            return await _http.GetJsonAsync<List<WorksheetsDto>?>($"Worksheet/DB_GetWorkSheetsbyWorkpieceName/{WorkpieceName}");
        }
        public async Task<List<WorksheetsDto>?> DB_GetWorkSheetsbyContainWorkpieceName(string WorkpieceName, CancellationToken ct = default)
        {
            return await _http.GetJsonAsync<List<WorksheetsDto>?>($"Worksheet/DB_GetWorkSheetsbyContainWorkpieceName/{WorkpieceName}");
        }
        //====PUT====
    }
}
