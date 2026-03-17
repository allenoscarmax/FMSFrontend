using FMSFrontend.Features.Dtos;
using FMSFrontend.Services;
using System.Threading;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Services
{
    public interface IAMRService
    {
        Task<AMRDto?> GetAgvParaAsync(CancellationToken ct = default);
    }

    public class AMRService : IAMRService
    {
        private readonly IHttpService _http;

        public AMRService(IHttpService http) => _http = http;

        public async Task<AMRDto?> GetAgvParaAsync(CancellationToken ct = default)
            => await _http.GetJsonAsync<AMRDto>("AMR_ITRI/GetAgvPara", ct);
    }
}
