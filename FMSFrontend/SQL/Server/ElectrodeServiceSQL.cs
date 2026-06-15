using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Services;
using FMSFrontend.SQL.Dtos;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FMSFrontend.SQL.Server
{
    public class ElectrodeServiceSQL : IElectrodeService
    {
        private readonly ISqlServer _sqlServer;

        public ElectrodeServiceSQL(ISqlServer sqlServer) => _sqlServer = sqlServer;

        public async Task<List<ElectrodeInventoryDto>?> DB_GetAllElectrodeAsync(CancellationToken ct = default)
        {
            var result = new List<ElectrodeInventoryDto>();

            await using var conn = _sqlServer.CreateConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
                    SELECT [ElectrodeID], [ElectrodeType], [Location], [MaxUsageCount], [CurrentUsageCount], [Status], [CurrentStatus], [OffsetsJson], [HomeLocation], [LastUpdated]
                    FROM [dbo].[ElectrodeInventory]
                    ORDER BY [LastUpdated];";

            await using var cmd = new SqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                result.Add(MapElectrodeInventory(reader));
            }

            return result;
        }

        private static ElectrodeInventoryDto MapElectrodeInventory(SqlDataReader reader)
        {
            var dto = new ElectrodeInventoryDto
            {
                ElectrodeID = reader["ElectrodeID"]?.ToString() ?? string.Empty,
                ElectrodeType = reader["ElectrodeType"]?.ToString() ?? string.Empty,
                Location = reader["Location"]?.ToString() ?? string.Empty,
                MaxUsageCount = reader["MaxUsageCount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["MaxUsageCount"]),
                CurrentUsageCount = reader["CurrentUsageCount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CurrentUsageCount"]),
                Status = reader["Status"]?.ToString() ?? string.Empty,
                CurrentStatus = reader["CurrentStatus"]?.ToString() ?? string.Empty,
                OffsetsJson = reader["OffsetsJson"]?.ToString() ?? string.Empty,
                HomeLocation = reader["HomeLocation"]?.ToString() ?? string.Empty,
                LastUpdated = reader["LastUpdated"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["LastUpdated"])
            };

            return dto;
        }

        private static ElectrodeDto MapInventoryToFeature(ElectrodeInventoryDto inv)
        {
            var dto = new ElectrodeDto
            {
                _id = inv.ElectrodeID ?? string.Empty,
                tagSerial = inv.ElectrodeID ?? string.Empty,
                electrodeName = inv.ElectrodeID ?? string.Empty,
                electrodeType = inv.ElectrodeType ?? string.Empty,
                currentLocation = inv.Location ?? string.Empty,
                state = !string.IsNullOrEmpty(inv.CurrentStatus) ? inv.CurrentStatus : inv.Status ?? string.Empty,
                lifeTimes = inv.MaxUsageCount,
                useTimes = inv.CurrentUsageCount,
                tempRetSLocation = inv.HomeLocation ?? string.Empty
            };

            ApplyOffsetsJson(dto, inv.OffsetsJson);
            return dto;
        }

        async Task<bool> IElectrodeService.DB_UpdateElectrodeDataAsync(ElectrodeDto dto, CancellationToken ct)
        {
            if (dto == null) return false;
            var inv = new ElectrodeInventoryDto
            {
                ElectrodeID = dto.tagSerial ?? "",
                ElectrodeType = dto.electrodeType ?? "",
                Location = dto.currentLocation ?? "",
                MaxUsageCount = dto.lifeTimes ?? 0,
                CurrentUsageCount = dto.useTimes ?? 0,
                Status = dto.state ?? "",
                CurrentStatus = dto.state ?? "",
                OffsetsJson = BuildOffsetsJson(dto),
                HomeLocation = dto.tempRetSLocation ?? "",
                LastUpdated = DateTime.UtcNow
            };

            return await DB_UpdateElectrodeDataAsync(inv, ct);
        }

        async Task<List<ElectrodeDto>?> IElectrodeService.GetElectrodeByWorksheetNumberAsync(string WorksheetNumber, CancellationToken ct)
        {
            var inv = await GetElectrodeByWorksheetNumberAsync(WorksheetNumber, ct);
            if (inv == null) return null;
            var list = new List<ElectrodeDto>();
            foreach (var i in inv) list.Add(MapInventoryToFeature(i));
            return list;
        }

        async Task<List<ElectrodeDto>?> IElectrodeService.DB_GetElectrodesByTagSerialAsync(string tagSerial, CancellationToken ct)
        {
            var inv = await DB_GetElectrodesByTagSerialAsync(tagSerial, ct);
            if (inv == null) return null;
            var list = new List<ElectrodeDto>();
            foreach (var i in inv) list.Add(MapInventoryToFeature(i));
            return list;
        }

        // IElectrodeService implementation (maps inventory DTOs to feature DTOs)
        async Task<List<ElectrodeDto>?> IElectrodeService.DB_GetAllElectrodeAsync(CancellationToken ct)
        {
            var inv = await DB_GetAllElectrodeAsync(ct);
            if (inv == null) return null;
            var list = new List<ElectrodeDto>();
            foreach (var i in inv) list.Add(MapInventoryToFeature(i));
            return list;
        }

        public async Task<List<ElectrodeInventoryDto>?> DB_GetElectrodesByTagSerialAsync(string tagSerial, CancellationToken ct = default)
        {
            var result = new List<ElectrodeInventoryDto>();

            await using var conn = _sqlServer.CreateConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
                        SELECT [ElectrodeID], [ElectrodeType], [Location], [MaxUsageCount], [CurrentUsageCount], [Status], [CurrentStatus], [OffsetsJson], [HomeLocation], [LastUpdated]
                        FROM [dbo].[ElectrodeInventory]
                        WHERE [ElectrodeID] = @ElectrodeID
                        ORDER BY [LastUpdated] DESC;";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ElectrodeID", tagSerial ?? string.Empty);
            await using var reader = await cmd.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                result.Add(MapElectrodeInventory(reader));
            }

            return result;
        }

        public async Task<List<ElectrodeInventoryDto>?> GetElectrodeByWorksheetNumberAsync(string WorksheetNumber, CancellationToken ct = default)
        {
            var result = new List<ElectrodeInventoryDto>();

            await using var conn = _sqlServer.CreateConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
SELECT [ElectrodeID], [ElectrodeType], [Location], [MaxUsageCount], [CurrentUsageCount], [Status], [CurrentStatus], [OffsetsJson], [HomeLocation], [LastUpdated]
FROM [dbo].[ElectrodeInventory]
WHERE [ElectrodeID] LIKE @Keyword OR [OffsetsJson] LIKE @Keyword
ORDER BY [LastUpdated] DESC;";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Keyword", $"%{WorksheetNumber}%");
            await using var reader = await cmd.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                result.Add(MapElectrodeInventory(reader));
            }

            return result;
        }

        public async Task<bool> DB_UpdateElectrodeDataAsync(ElectrodeInventoryDto dto, CancellationToken ct = default)
        {
            await using var conn = _sqlServer.CreateConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
                                UPDATE [dbo].[ElectrodeInventory]
                                SET [ElectrodeType] = @ElectrodeType,
                                    [Location] = @Location,
                                    [MaxUsageCount] = @MaxUsageCount,
                                    [CurrentUsageCount] = @CurrentUsageCount,
                                    [Status] = @Status,
                                    [CurrentStatus] = @CurrentStatus,
                                    [OffsetsJson] = @OffsetsJson,
                                    [HomeLocation] = @HomeLocation,
                                    [LastUpdated] = SYSUTCDATETIME()
                                WHERE [ElectrodeID] = @ElectrodeID;";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ElectrodeType", dto.ElectrodeType ?? string.Empty);
            cmd.Parameters.AddWithValue("@Location", dto.Location ?? string.Empty);
            cmd.Parameters.AddWithValue("@MaxUsageCount", dto.MaxUsageCount);
            cmd.Parameters.AddWithValue("@CurrentUsageCount", dto.CurrentUsageCount);
            cmd.Parameters.AddWithValue("@Status", dto.Status ?? string.Empty);
            cmd.Parameters.AddWithValue("@CurrentStatus", dto.CurrentStatus ?? string.Empty);
            cmd.Parameters.AddWithValue("@OffsetsJson", dto.OffsetsJson ?? string.Empty);
            cmd.Parameters.AddWithValue("@HomeLocation", dto.HomeLocation ?? string.Empty);
            cmd.Parameters.AddWithValue("@ElectrodeID", dto.ElectrodeID ?? string.Empty);
            return await cmd.ExecuteNonQueryAsync(ct) > 0;
        }

        async Task<bool> IElectrodeService.DB_DeleteElectrodeDataByIdAsync(string id, CancellationToken ct)
            => await DB_DeleteElectrodeDataByIdAsync(id, ct);

        public async Task<bool> DB_SetElectrodeRestrictionByTagSerialAsync(string tagSerial, bool restriction, CancellationToken ct = default)
        {
            var electrodes = await DB_GetElectrodesByTagSerialAsync(tagSerial, ct);
            var dto = electrodes?.Count > 0 ? electrodes[0] : null;
            if (dto == null)
                return false;

            // Update the OffsetsJson 'restriction' field
            dto.OffsetsJson = UpdateOffsetsRestriction(dto.OffsetsJson, restriction);
            return await DB_UpdateElectrodeDataAsync(dto, ct);
        }

        async Task<List<EleTimelineDto>?> IElectrodeService.DB_GetElectrodeTimelineByIdAsync(string id, CancellationToken ct)
            => await Task.FromResult<List<EleTimelineDto>?>(new List<EleTimelineDto>());

        private static string UpdateOffsetsRestriction(string? offsetsJson, bool restriction)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(offsetsJson))
                {
                    var payload = new Dictionary<string, object?> { ["restriction"] = restriction };
                    return JsonSerializer.Serialize(payload);
                }

                using var doc = JsonDocument.Parse(offsetsJson);
                var root = doc.RootElement;
                var dict = new Dictionary<string, object?>();
                foreach (var prop in root.EnumerateObject())
                {
                    dict[prop.Name] = prop.Value.ValueKind switch
                    {
                        JsonValueKind.Number => prop.Value.TryGetInt32(out var i) ? (object?)i : prop.Value.GetRawText(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.String => prop.Value.GetString(),
                        _ => prop.Value.GetRawText()
                    };
                }

                dict["restriction"] = restriction;
                return JsonSerializer.Serialize(dict);
            }
            catch
            {
                return JsonSerializer.Serialize(new Dictionary<string, object?> { ["restriction"] = restriction });
            }
        }

        async Task<bool> IElectrodeService.DB_SetElectrodeRestrictionByTagSerialAsync(string tagSerial, bool restriction, CancellationToken ct)
            => await DB_SetElectrodeRestrictionByTagSerialAsync(tagSerial, restriction, ct);

        public async Task<bool> DB_DeleteElectrodeDataByIdAsync(string id, CancellationToken ct = default)
        {
            await using var conn = _sqlServer.CreateConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
DELETE FROM [dbo].[ElectrodeInventory]
WHERE [ElectrodeID] = @ElectrodeID;";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ElectrodeID", id);

            return await cmd.ExecuteNonQueryAsync(ct) > 0;
        }

        private static ElectrodeInventoryDto MapElectrodeInventory(SqlDataReader reader)
        {
            var dto = new ElectrodeInventoryDto
            {
                ElectrodeID = reader["ElectrodeID"]?.ToString() ?? string.Empty,
                ElectrodeType = reader["ElectrodeType"]?.ToString() ?? string.Empty,
                Location = reader["Location"]?.ToString() ?? string.Empty,
                MaxUsageCount = reader["MaxUsageCount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["MaxUsageCount"]),
                CurrentUsageCount = reader["CurrentUsageCount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CurrentUsageCount"]),
                Status = reader["Status"]?.ToString() ?? string.Empty,
                CurrentStatus = reader["CurrentStatus"]?.ToString() ?? string.Empty,
                OffsetsJson = reader["OffsetsJson"]?.ToString() ?? string.Empty,
                HomeLocation = reader["HomeLocation"]?.ToString() ?? string.Empty,
                LastUpdated = reader["LastUpdated"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["LastUpdated"])
            };

            return dto;
        }

        private static void ApplyOffsetsJson(ElectrodeDto dto, string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return;

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                dto.worksheetNumber = GetString(root, "worksheetNumber");
                dto.offset = GetString(root, "offset");
                dto.offsetStatus = GetNullableInt(root, "offsetStatus");
                dto.restriction = GetNullableBool(root, "restriction");
                dto.edmpgm = GetString(root, "edmpgm");
                dto.edM_offsetPGM = GetString(root, "edM_offsetPGM");
                dto.measuremented = GetNullableBool(root, "measuremented");
                dto.measurementStatus = GetString(root, "measurementStatus");
                dto.shared = GetBool(root, "shared");
                dto.shareLink = GetString(root, "shareLink");
                dto.setupUser = GetString(root, "setupUser");
                dto.pairedEDM = GetString(root, "pairedEDM");

                var electrodeName = GetString(root, "electrodeName");
                if (!string.IsNullOrWhiteSpace(electrodeName))
                    dto.electrodeName = electrodeName;
            }
            catch
            {
            }
        }

        private static string BuildOffsetsJson(ElectrodeDto dto)
        {
            var payload = new Dictionary<string, object?>
            {
                ["worksheetNumber"] = dto.worksheetNumber,
                ["electrodeName"] = dto.electrodeName,
                ["offset"] = dto.offset,
                ["offsetStatus"] = dto.offsetStatus,
                ["restriction"] = dto.restriction,
                ["edmpgm"] = dto.edmpgm,
                ["edM_offsetPGM"] = dto.edM_offsetPGM,
                ["measuremented"] = dto.measuremented,
                ["measurementStatus"] = dto.measurementStatus,
                ["shared"] = dto.shared,
                ["shareLink"] = dto.shareLink,
                ["setupUser"] = dto.setupUser,
                ["pairedEDM"] = dto.pairedEDM
            };

            return JsonSerializer.Serialize(payload);
        }

        private static string GetString(JsonElement root, string propertyName)
            => root.TryGetProperty(propertyName, out var value) ? value.ToString() : string.Empty;

        private static int? GetNullableInt(JsonElement root, string propertyName)
            => root.TryGetProperty(propertyName, out var value) && value.TryGetInt32(out var result) ? result : null;

        private static bool? GetNullableBool(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out var value))
                return null;

            return value.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => null
            };
        }

        private static bool GetBool(JsonElement root, string propertyName)
            => GetNullableBool(root, propertyName) ?? false;
    }
}
