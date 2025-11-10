using FMSFrontend.Features.Dtos;
using FMSFrontend.Services;
using System.Threading;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Services
{
    public interface IStorageService
    {
        Task<List<Storage>?> GetAllStorageAsync(CancellationToken ct = default); //取得所有電極資料
        Task<bool> UpdateStorageDataAsync(Storage StorageDto, CancellationToken ct = default); //更新電極資料
    }

    public class StorageService : IStorageService
    {
        private readonly IHttpService _http;
        public StorageService(IHttpService http) => _http = http;
        //====GET====
        public async Task<List<Storage>?> GetAllStorageAsync(CancellationToken ct = default) //取得所有電極資料
        {
            return await _http.GetJsonAsync<List<Storage>>("Storage/DB_GetAllStorageData", ct);
        }
        //====PUT====
        public async Task<bool> UpdateStorageDataAsync(Storage StorageDto, CancellationToken ct = default) //更新電極資料
        {
            return await _http.SendPutAsync($"Storage/DB_UpdateStorageData", StorageDto);
        }
    }
}
