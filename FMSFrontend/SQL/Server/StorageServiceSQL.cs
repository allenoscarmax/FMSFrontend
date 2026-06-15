using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Services;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FMSFrontend.SQL.Server
{
    public class StorageServiceSQL : IStorageService
    {
        private readonly ISqlServer _sqlServer;

        public StorageServiceSQL(ISqlServer sqlServer) => _sqlServer = sqlServer;

        public async Task<List<StorageDto>?> GetAllStorageAsync(CancellationToken ct = default)
        {
            var result = new List<StorageDto>();

            await using var conn = _sqlServer.CreateConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
                SELECT [LocationCode], [WarehouseName], [RackID], [ZoneID], [RowIdx], [LayerIdx], [TagID], [SlotStatus], [IsEnabled], [LastErrorMessage]
                FROM [dbo].[StorageSlot]
                WHERE [WarehouseName] IN ('E', 'W', 'ER')
                ORDER BY [WarehouseName], [RackID], [ZoneID], [RowIdx], [LayerIdx];";

            await using var cmd = new SqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                result.Add(new StorageDto
                {
                    storageName = reader["WarehouseName"]?.ToString() ?? string.Empty,
                    storageNumber = reader["RackID"]?.ToString() ?? string.Empty,
                    region = GetInt(reader, "ZoneID"),
                    column = GetInt(reader, "RowIdx"),
                    row = GetInt(reader, "LayerIdx"),
                    ondeskTagserial = reader["TagID"]?.ToString() ?? string.Empty,
                    state = MapStorageState(reader["SlotStatus"]?.ToString()),
                    restriction = !GetBool(reader, "IsEnabled"),
                });
            }

            return result;
        }

        public async Task<StorageDto?> GetStorageByLocationAsync(string storageName, string storageNumber, int region, int column, int row, CancellationToken ct = default)
        {
            var all = await GetAllStorageAsync(ct);
            return all?.FirstOrDefault(x =>
                x.storageName == storageName &&
                x.storageNumber == storageNumber &&
                x.region == region &&
                x.column == column &&
                x.row == row);
        }

        public async Task<bool> UpdateStorageDataAsync(StorageDto storageDto, CancellationToken ct = default)
        {
            await using var conn = _sqlServer.CreateConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
UPDATE [dbo].[StorageSlot]
SET [TagID] = @TagID,
    [SlotStatus] = @SlotStatus,
    [IsEnabled] = @IsEnabled,
    [LastErrorMessage] = @LastErrorMessage,
    [LastUpdated] = SYSUTCDATETIME()
WHERE [WarehouseName] = @WarehouseName
  AND [RackID] = @RackID
  AND [ZoneID] = @ZoneID
  AND [RowIdx] = @RowIdx
  AND [LayerIdx] = @LayerIdx;";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@TagID", (object?)storageDto.ondeskTagserial ?? System.DBNull.Value);
            cmd.Parameters.AddWithValue("@SlotStatus", MapStorageStateToDb(storageDto.state));
            cmd.Parameters.AddWithValue("@IsEnabled", !(storageDto.restriction ?? false));
            cmd.Parameters.AddWithValue("@LastErrorMessage", (object?)storageDto.note ?? System.DBNull.Value);
            cmd.Parameters.AddWithValue("@WarehouseName", storageDto.storageName);
            cmd.Parameters.AddWithValue("@RackID", storageDto.storageNumber);
            cmd.Parameters.AddWithValue("@ZoneID", storageDto.region);
            cmd.Parameters.AddWithValue("@RowIdx", storageDto.column);
            cmd.Parameters.AddWithValue("@LayerIdx", storageDto.row);

            return await cmd.ExecuteNonQueryAsync(ct) > 0;
        }

        public async Task<bool> SetRestrictionByLocationAsync(string storageName, string storageNumber, int region, int column, int row, bool restriction, CancellationToken ct = default)
        {
            await using var conn = _sqlServer.CreateConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
UPDATE [dbo].[StorageSlot]
SET [IsEnabled] = @IsEnabled,
    [LastUpdated] = SYSUTCDATETIME()
WHERE [WarehouseName] = @WarehouseName
  AND [RackID] = @RackID
  AND [ZoneID] = @ZoneID
  AND [RowIdx] = @RowIdx
  AND [LayerIdx] = @LayerIdx;";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@IsEnabled", !restriction);
            cmd.Parameters.AddWithValue("@WarehouseName", storageName);
            cmd.Parameters.AddWithValue("@RackID", storageNumber);
            cmd.Parameters.AddWithValue("@ZoneID", region);
            cmd.Parameters.AddWithValue("@RowIdx", column);
            cmd.Parameters.AddWithValue("@LayerIdx", row);

            return await cmd.ExecuteNonQueryAsync(ct) > 0;
        }

        private static int GetInt(SqlDataReader reader, string name)
            => reader[name] == System.DBNull.Value ? 0 : System.Convert.ToInt32(reader[name]);

        private static bool GetBool(SqlDataReader reader, string name)
            => reader[name] != System.DBNull.Value && System.Convert.ToBoolean(reader[name]);

        private static string MapStorageState(string? slotStatus)
            => string.IsNullOrWhiteSpace(slotStatus) ? "Vacant" : slotStatus;

        private static string MapStorageStateToDb(string? state)
            => string.IsNullOrWhiteSpace(state) ? "Vacant" : state;
    }
}
