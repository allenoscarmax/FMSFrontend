using FMSFrontend.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Services
{
    public interface IMongoDBService
    {
        /// <summary>
        /// 執行 MongoDB 資料庫備份
        /// </summary>
        Task<bool> BackupDatabaseAsync(CancellationToken ct = default);

        /// <summary>
        /// 從指定路徑還原 MongoDB 備份
        /// </summary>
        Task<bool> RestoreDatabaseAsync(string backupPath, CancellationToken ct = default);
    }

    public class MongoDBService : IMongoDBService
    {
        private readonly IHttpService _http;
        public MongoDBService(IHttpService http) => _http = http;

        /// <summary>
        /// 執行資料庫備份
        /// 對應後端 API: PUT /MongoDB/DB_Backup
        /// </summary>
        public async Task<bool> BackupDatabaseAsync(CancellationToken ct = default)
        {
            return await _http.SendPutAsync("MongoDB/DB_Backup", new { });
        }

        /// <summary>
        /// 從指定備份路徑還原 MongoDB
        /// 對應後端 API: PUT /MongoDB/DB_RestoreMongo/{backupPath}
        /// </summary>
        public async Task<bool> RestoreDatabaseAsync(string backupPath, CancellationToken ct = default)
        {
            var route = $"MongoDB/DB_RestoreMongo/{Uri.EscapeDataString(backupPath)}";
            return await _http.SendPutAsync(route, new { });
        }
    }
}
