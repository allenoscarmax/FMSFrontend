using FMSFrontend.Features.Dtos;
using FMSFrontend.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using static MaterialDesignThemes.Wpf.Theme;
using static System.Net.WebRequestMethods;

namespace FMSFrontend.Features.Services
{
    public interface IRFIDMgmtModuleService
    {
        //====GET====
        public Task<RFIDParasDto?> GetRFIDParasAsync(CancellationToken ct = default);//讀取RFID參數
        public Task<string?> Read_Tag_IDAsync(int index, int header, CancellationToken ct = default);  //讀取RFID標籤ID
        public Task<RFIDWriteLogDto?> GetAllRFIDWriteLogAsync(CancellationToken ct = default);//取得所有燒錄記錄

        //====PUT====
        public Task<bool> DeleteAllRFIDWriteLogDataAsync(); //刪除所有燒錄記錄
        public Task<bool> InsertNewRFIDWriteLogDataAsync(RFIDWriteLogDto RFIDWriteLog, CancellationToken ct = default); //新增燒錄記錄

    }

    public class RFIDMgmtModuleService : IRFIDMgmtModuleService
    {
        private readonly IHttpService _http;
        public RFIDMgmtModuleService(IHttpService http) => _http = http;
        //====GET====  
        public async Task<RFIDParasDto?> GetRFIDParasAsync(CancellationToken ct = default)  //讀取RFID標籤ID
        {
            return await _http.GetJsonAsync<RFIDParasDto>($"RFIDMgmtModule/GetRFIDParas", ct);
        }
        public async Task<string?> Read_Tag_IDAsync(int index, int header, CancellationToken ct = default)  //讀取RFID標籤ID
        {
            var route = $"RFIDMgmtModule/Read_Tag_ID/{index}/{header}";
            return await _http.GetJsonAsync<string>(route, ct);
        }
        public async Task<RFIDWriteLogDto?> GetAllRFIDWriteLogAsync(CancellationToken ct = default)//取得所有燒錄記錄
        {
            return await _http.GetJsonAsync<RFIDWriteLogDto>("RFIDMgmtModule/DB_GetAllRFIDWriteLog", ct);
        }

        //====PUT====
        public async Task<bool> DeleteAllRFIDWriteLogDataAsync()      //刪除所有燒錄記錄
        {
            var route = $"RFIDMgmtModule/DB_DeleteAllRFIDWriteLogData";
            return await _http.SendPutAsync(route, new { });
        }        
        public async Task<bool> InsertNewRFIDWriteLogDataAsync(RFIDWriteLogDto RFIDWriteLog, CancellationToken ct = default)  //新增燒錄記錄
        {
            var route = $"RFIDMgmtModule/DB_InsertNewRFIDWriteLogData";
            return await _http.SendPutAsync(route, RFIDWriteLog);
        }
    }
}
