using FMSFrontend.Features.Dtos;
using FMSFrontend.Services;
using OSCARMAXFMS_V3.DBmodels;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FMSFrontend.Features.Services
{
    public interface IElectrodeService
    {
        Task<List<ElectrodeDto>?> GetAllElectrodeAsync(CancellationToken ct = default); //取得所有電極資料
        Task<bool> UpdateElectrodeDataAsync(ElectrodeDto electrodeDto, CancellationToken ct = default); //更新電極資料
    }

    public class ElectrodeService : IElectrodeService
    {
        private readonly IHttpService _http;
        public ElectrodeService(IHttpService http) => _http = http;
        //====GET====
        public async Task<List<ElectrodeDto>?> GetAllElectrodeAsync(CancellationToken ct = default) //取得所有電極資料
        {
            return await _http.GetJsonAsync<List<ElectrodeDto>>("Electrode/DB_GetAllElectrode", ct);
        }
        public async Task<List<ElectrodeDto>?> GetElectrodesbyTagSerialAsync(string TagSerial, CancellationToken ct = default) //取得所有電極資料
        {
            return await _http.GetJsonAsync<List<ElectrodeDto>>($"Electrode/DB_GetElectrodesbyTagSerial/{TagSerial}", ct);
        }

        public async Task<List<EleTimelineDto>?> GetElectrodeTimelinebyIdAsync(string id, CancellationToken ct = default) //取得電極加工進度
        {
            var route = $"Electrode/DB_GetElectrodeTimelinebyId/{id}";
            return await _http.GetJsonAsync<List<EleTimelineDto>>(route, ct);
        }

        //====PUT====
        public async Task<bool> UpdateElectrodeDataAsync(ElectrodeDto electrodeDto, CancellationToken ct = default) //更新電極資料
        {
            return await _http.SendPutAsync($"Electrode/DB_UpdateElectrodeData", electrodeDto);
        }
    }
}
