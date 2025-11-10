using FMSFrontend.Features.Dtos;
using FMSFrontend.Services;
using System.Threading;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Services
{
    public interface IMachinesService
    {
        Task<List<MachinesDto>?> GetAllMachinesAsync(CancellationToken ct = default); //取得所有電極資料
        Task<bool> UpdateMachinesDataAsync(MachinesDto MachinesDto, CancellationToken ct = default); //更新電極資料
    }

    public class MachinesService : IMachinesService
    {
        private readonly IHttpService _http;
        public MachinesService(IHttpService http) => _http = http;
        //====GET====
        public async Task<List<MachinesDto>?> GetAllMachinesAsync(CancellationToken ct = default) //取得所有電極資料
        {
            return await _http.GetJsonAsync<List<MachinesDto>>("Machine/DB_GetAllMachines", ct);
        }
        //====PUT====
        public async Task<bool> UpdateMachinesDataAsync(MachinesDto MachinesDto, CancellationToken ct = default) //更新電極資料
        {
            return await _http.SendPutAsync($"Machine/DB_UpdateMachineData", MachinesDto);
        }
    }
}
