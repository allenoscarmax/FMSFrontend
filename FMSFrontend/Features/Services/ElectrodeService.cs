using FMSFrontend.Features.Dtos;
using FMSFrontend.Services;
using System.Threading;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Services
{
    public interface IElectrodeService
    {
        Task<ElectrodeDto?> GetAllElectrodeAsync(CancellationToken ct = default); //取得所有電極資料
        Task<bool> UpdateElectrodeDataAsync(ElectrodeDto electrodeDto, CancellationToken ct = default); //更新電極資料
    }

    public class ElectrodeService : IElectrodeService
    {
        private readonly IHttpService _http;
        public ElectrodeService(IHttpService http) => _http = http;
        //====GET====
        public async Task<ElectrodeDto?> GetAllElectrodeAsync(CancellationToken ct = default) //取得所有電極資料
        {
            return await _http.GetJsonAsync<ElectrodeDto>("Electrode/DB_GetAllElectrode", ct);
        }
        //====PUT====
        public async Task<bool> UpdateElectrodeDataAsync(ElectrodeDto electrodeDto, CancellationToken ct = default) //更新電極資料
        {
            return await _http.SendPutAsync($"Electrode/DB_UpdateElectrodeData", electrodeDto);
        }
    }
}
