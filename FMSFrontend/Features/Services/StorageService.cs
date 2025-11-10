using FMSFrontend.Features.Dtos;
using FMSFrontend.Services;
using System.Threading;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    namespace FMSFrontend.Features.Services
    {
        public interface IStorageService
        {
            // ==== Storage 區 ====
            Task<List<StorageDto>?> GetAllStorageAsync(CancellationToken ct = default);                                     // GET Storage/DB_GetAllStorageData
            Task<bool> UpdateStorageDataAsync(StorageDto storageDto, CancellationToken ct = default);                       // PUT Storage/DB_UpdateStorageData

            Task<bool> SetOndeskTagserialByLocationAsync(                                                               // PUT Storage/DB_SetOndeskTagserialbyStorageLocation?StorageName=...&...
                string storageName, string storageNumber, int region, int column, int row, string ondeskTagserial, CancellationToken ct = default);

            Task<StorageDto?> GetStorageByLocationAsync(                                                                    // GET Storage/DB_GetStoragebyStorageLocation/{StorageName}/{StorageNumber}/{Region}/{Column}/{Row}
                string storageName, string storageNumber, int region, int column, int row, CancellationToken ct = default);

            Task<List<StorageDto>?> GetStorageByNameAsync(string storageName, string storageNumber, CancellationToken ct = default); // GET Storage/DB_GetStoragebyStorageName/{StorageName}/{StorageNumber}

            Task<bool> RemoveOndeskTagserialFromAllLocationAsync(string tagSerial, CancellationToken ct = default);      // PUT Storage/DB_RemoveOndeskTagserialFromAllLocation/{TagSerial}
            Task<bool> RemoveAllOndeskTagserialAsync(CancellationToken ct = default);                                    // PUT Storage/DB_RemoveAlldeskTagserialFromAllLocation

            Task<bool> SetRestrictionByLocationAsync(                                                                    // PUT Storage/DB_SetRestrictionbyStorageLocation/{StorageName}/{StorageNumber}/{Region}/{Column}/{Row}/{Restriction}
                string storageName, string storageNumber, int region, int column, int row, bool restriction, CancellationToken ct = default);
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

}
