using FMSFrontend.Features.Dtos;
using FMSFrontend.Services;
using System.Threading;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Services
{
    public interface IWorksheetsService
    {
        public Task<List<WorksheetsDto>?> GetAllWorkSheetAsync(CancellationToken ct = default);
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
        //====PUT====
    }
}
