using ControlzEx.Standard;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Services
{

    public interface IOperationMessageLogService
    {
        // ==== GET ====
        Task<List<OperationMessageLogDto>?> GetAllOperationMessageLogAsync(CancellationToken ct = default);
        Task<List<OperationMessageLogDto>?> GetOperationMessageLogByDateAsync(DateTime startTime, DateTime endTime, CancellationToken ct = default);

        // ==== PUT ====
        Task<bool> InsertNewOperationMessageLogDataAsync(OperationMessageLogDto payload, CancellationToken ct = default);
        Task<bool> DeleteOperationMessageLogDataByIdAsync(string id, CancellationToken ct = default);
        Task<bool> DeleteAllOperationMessageLogDataAsync(CancellationToken ct = default);
    }

    public class OperationMessageLogService : IOperationMessageLogService
    {
        private readonly IHttpService _http;
        public OperationMessageLogService(IHttpService http) => _http = http;

        // ==== GET ====
        public async Task<List<OperationMessageLogDto>?> GetAllOperationMessageLogAsync(CancellationToken ct = default)
            => await _http.GetJsonAsync<List<OperationMessageLogDto>>("OperationLog/DB_GetAllOperationMessageLog", ct);

        public async Task<List<OperationMessageLogDto>?> GetOperationMessageLogByDateAsync(DateTime startTime, DateTime endTime, CancellationToken ct = default)
        {
            // 跟你後台路由一致：DB_GetOperationMessageLogByDate/{StartTime}/{EndTime}
            // 用 "O" (round-trip) 避免文化/格式問題，並 Escape
            string s = Uri.EscapeDataString(startTime.ToString("O"));
            string e = Uri.EscapeDataString(endTime.ToString("O"));
            return await _http.GetJsonAsync<List<OperationMessageLogDto>>($"OperationLog/DB_GetOperationMessageLogByDate/{s}/{e}", ct);
        }

        // ==== PUT ====
        public async Task<bool> InsertNewOperationMessageLogDataAsync(OperationMessageLogDto payload, CancellationToken ct = default)
            => await _http.SendPutAsync("OperationLog/DB_InsertNewOperationMessageLogData", payload);

        public async Task<bool> DeleteOperationMessageLogDataByIdAsync(string id, CancellationToken ct = default)
            => await _http.SendPutAsync($"OperationLog/DB_DeleteOperationMessageLogDataById/{Uri.EscapeDataString(id)}", new { });

        public async Task<bool> DeleteAllOperationMessageLogDataAsync(CancellationToken ct = default)
            => await _http.SendPutAsync("OperationLog/DB_DeleteAllOperationMessageLogData", new { });
    }

    /*
    public interface IOperationService
    {
        
        Task WriteAsync(string data, string user, CancellationToken ct = default);
        Task<List<OperationDto>?> ReadAsync(DateTime form, DateTime to, CancellationToken ct = default);
    
    }
    public class OperationService : IOperationService
    {
        private readonly IHttpService _http;
        private string FolderPath = AppDomain.CurrentDomain.BaseDirectory + "Operation";
        public OperationService(IHttpService http) => _http = http;

        public async Task WriteAsync(string data, string user, CancellationToken ct = default) 
        {
            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);
            
            //寫入檔案 檔名為年加月.csv UTF8編碼帶BOM 每行格式為 時間,使用者,訊息
            string FilePath = FolderPath + $"\\{DateTime.Now.ToString("yyyyMM")}.csv";
            
            using (StreamWriter sw = new StreamWriter(FilePath, true, Encoding.UTF8))
            {
                await sw.WriteLineAsync($"{DateTime.Now},{user},{data}");
            }
        }
        public async Task<List<OperationDto>?> ReadAsync(DateTime form, DateTime to, CancellationToken ct = default)
        {
            //讀去from到to之間的月份 取出對應的資料寫入List<OperationDto>
            List<OperationDto> result = new List<OperationDto>();
            DateTime current = new DateTime(form.Year, form.Month, 1);
            DateTime end = new DateTime(to.Year, to.Month, 1);
            while (current <= end)
            {
                string FilePath = FolderPath + $"//{current.ToString("yyyyMM")}.csv";
                if (File.Exists(FilePath))
                {
                    using (StreamReader sr = new StreamReader(FilePath, Encoding.UTF8))
                    {
                        string? line;
                        while ((line = await sr.ReadLineAsync()) != null)
                        {
                            var parts = line.Split(',');
                            if (parts.Length < 3)
                                continue;

                            if (!DateTime.TryParse(parts[0], out var time))
                                continue;

                            if (time >= form && time <= to)
                            {
                                result.Add(new OperationDto
                                {
                                    Time = time,
                                    Operation = parts[1],
                                    worksheetDone = parts[2]
                                });
                            }
                        }
                    }
                }
                current = current.AddMonths(1);
            }
            return result;
        }
    }*/
}
