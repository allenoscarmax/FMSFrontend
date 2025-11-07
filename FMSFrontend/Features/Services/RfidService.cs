using FMSFrontend.Features.Dtos;
using FMSFrontend.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using static MaterialDesignThemes.Wpf.Theme;
using static System.Net.WebRequestMethods;

namespace FMSFrontend.Features.Services
{
    public interface IRfidService
    {
        //====GET====
        public Task<RFIDParasDto?> GetRFIDParasAsync(CancellationToken ct = default);//讀取RFID參數
        public Task<string?> Read_Tag_IDAsync(int index, int header, CancellationToken ct = default);  //讀取RFID標籤ID
        public Task<List<RFIDWriteLogDto>?> GetAllRFIDWriteLogAsync(CancellationToken ct = default);//取得所有燒錄記錄

        //====PUT====
        public Task<bool> DeleteAllRFIDWriteLogDataAsync(); //刪除所有燒錄記錄
        public Task<bool> InsertNewRFIDWriteLogDataAsync(RFIDWriteLogDto RFIDWriteLog, CancellationToken ct = default); //新增燒錄記錄

    }

    public class RfidService : IRfidService
    {
        private readonly IHttpService _http;
        public RfidService(IHttpService http) => _http = http;
        //====GET====  
        public async Task<RFIDParasDto?> GetRFIDParasAsync(CancellationToken ct = default)  //讀取RFID標籤ID
        {
            var json = await _http.GetJsonAsync<RFIDParasDto>($"RFIDMgmtModule/GetRFIDParas", ct);
            return json;
        }
        public async Task<string?> Read_Tag_IDAsync(int index, int header, CancellationToken ct = default)  //讀取RFID標籤ID
        {
            var route = $"RFIDMgmtModule/Read_Tag_ID/{index}/{header}";
            return await _http.GetJsonAsync<string>(route, ct);
        }
        public async Task<List<RFIDWriteLogDto>?> GetAllRFIDWriteLogAsync(CancellationToken ct = default)//取得所有燒錄記錄
        {
            var json = await _http.GetJsonAsync<List<RFIDWriteLogDto>>("RFIDMgmtModule/DB_GetAllRFIDWriteLog", ct);
            return json;
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
