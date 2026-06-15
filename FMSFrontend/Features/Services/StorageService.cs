using FMSFrontend.Features.Dtos;
using FMSFrontend.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Services
{
    public interface IStorageService
    {
        // ==== Storage 區 ====
        Task<List<StorageDto>?> GetAllStorageAsync(CancellationToken ct = default);     //取得所有倉儲資料                               
        Task<StorageDto?> GetStorageByLocationAsync(string storageName, string storageNumber, int region, int column, int row, CancellationToken ct = default); //根據位置取得倉儲資料
        Task<bool> UpdateStorageDataAsync(StorageDto storageDto, CancellationToken ct = default); //更新倉儲資料                      
        Task<bool> SetRestrictionByLocationAsync(string storageName, string storageNumber, int region, int column, int row, bool restriction, CancellationToken ct = default); //根據位置禁用倉儲
        /*
                Task<bool> SetOndeskTagserialByLocationAsync(string storageName, string storageNumber, int region, int column, int row, string ondeskTagserial, CancellationToken ct = default);
                Task<List<StorageDto>?> GetStorageByNameAsync(string storageName, string storageNumber, CancellationToken ct = default); 
                Task<bool> RemoveOndeskTagserialFromAllLocationAsync(string tagSerial, CancellationToken ct = default);  
                Task<bool> RemoveAllOndeskTagserialAsync(CancellationToken ct = default);                                 
        */
    }

    public class StorageService : IStorageService
    {
        private readonly IHttpService _http;
        public StorageService(IHttpService http) => _http = http;

        // ==== Storage 區 ====

        public async Task<List<StorageDto>?> GetAllStorageAsync(CancellationToken ct = default)
            => await _http.GetJsonAsync<List<StorageDto>>("Storage/DB_GetAllStorageData", ct);

        public async Task<bool> UpdateStorageDataAsync(StorageDto storageDto, CancellationToken ct = default)
            => await _http.SendPutAsync("Storage/DB_UpdateStorageData", storageDto);

        public async Task<bool> SetOndeskTagserialByLocationAsync(string storageName, string storageNumber, int region, int column, int row, string ondeskTagserial, CancellationToken ct = default)
            => await _http.SendPutAsync(
                $"Storage/DB_SetOndeskTagserialbyStorageLocation" +
                $"?StorageName={Uri.EscapeDataString(storageName)}" +
                $"&StorageNumber={Uri.EscapeDataString(storageNumber)}" +
                $"&Region={region}&Column={column}&Row={row}" +
                $"&OndeskTagserial={Uri.EscapeDataString(ondeskTagserial)}",
                new { });

        public async Task<StorageDto?> GetStorageByLocationAsync(string storageName, string storageNumber, int region, int column, int row, CancellationToken ct = default)
            => await _http.GetJsonAsync<StorageDto>(
                $"Storage/DB_GetStoragebyStorageLocation/{Uri.EscapeDataString(storageName)}/{Uri.EscapeDataString(storageNumber)}/{region}/{column}/{row}",
                ct);

        public async Task<List<StorageDto>?> GetStorageByNameAsync(string storageName, string storageNumber, CancellationToken ct = default)
            => await _http.GetJsonAsync<List<StorageDto>>(
                $"Storage/DB_GetStoragebyStorageName/{Uri.EscapeDataString(storageName)}/{Uri.EscapeDataString(storageNumber)}",
                ct);

        public async Task<bool> RemoveOndeskTagserialFromAllLocationAsync(string tagSerial, CancellationToken ct = default)
            => await _http.SendPutAsync($"Storage/DB_RemoveOndeskTagserialFromAllLocation/{Uri.EscapeDataString(tagSerial)}", new { });

        public async Task<bool> RemoveAllOndeskTagserialAsync(CancellationToken ct = default)
            => await _http.SendPutAsync("Storage/DB_RemoveAlldeskTagserialFromAllLocation", new { });

        public async Task<bool> SetRestrictionByLocationAsync(string storageName, string storageNumber, int region, int column, int row, bool restriction, CancellationToken ct = default)
            => await _http.SendPutAsync($"Storage/DB_SetRestrictionbyStorageLocation/{Uri.EscapeDataString(storageName)}/{Uri.EscapeDataString(storageNumber)}/{region}/{column}/{row}/{restriction}", new { });
    }

}
