using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Services;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FMSFrontend.SQL.Server
{
    public class WorkpieceServiceSQL : IWorkpieceService
    {
        private readonly ISqlServer _sqlServer;

        public WorkpieceServiceSQL(ISqlServer sqlServer) => _sqlServer = sqlServer;

        public async Task<List<WorkpieceDto>?> GetAllWorkpieceAsync(CancellationToken ct = default)
        {
            var result = new List<WorkpieceDto>();
            await using var conn = _sqlServer.CreateConnection();
            await conn.OpenAsync(ct);

            const string sql = @"
                    SELECT [WorkpieceTag], [PartNo], [BoundOrderID], [CurrentStatus], [CurrentLocation], [MeasuredDataJson], [HomeLocation]
                    FROM [dbo].[WorkpieceTracking]
                    ORDER BY [UpdatedAt] DESC;";

            await using var cmd = new SqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                var workpiece = MapWorkpiece(reader);
                if (workpiece != null)
                {
                    result.Add(workpiece);
                }
            }

            return result;
        }

        public async Task<WorkpieceDto?> GetWorkpieceByTagSerialAsync(string tagSerial, CancellationToken ct = default)
        {
            const string sql = @"
SELECT TOP (1) [WorkpieceTag], [PartNo], [BoundOrderID], [CurrentStatus], [CurrentLocation], [MeasuredDataJson], [HomeLocation]
FROM [dbo].[WorkpieceTracking]
WHERE [WorkpieceTag] = @WorkpieceTag
ORDER BY [UpdatedAt] DESC;";

            await using var conn = _sqlServer.CreateConnection();
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@WorkpieceTag", tagSerial);
            await using var reader = await cmd.ExecuteReaderAsync(ct);

            if (!await reader.ReadAsync(ct))
                return null;

            return MapWorkpiece(reader);
        }

        public async Task<WorkpieceDto?> GetWorkpieceByWorksheetNumberAsync(string worksheetNumber, CancellationToken ct = default)
        {
            const string sql = @"
SELECT TOP (1) [WorkpieceTag], [PartNo], [BoundOrderID], [CurrentStatus], [CurrentLocation], [MeasuredDataJson], [HomeLocation]
FROM [dbo].[WorkpieceTracking]
WHERE [BoundOrderID] = @BoundOrderID
ORDER BY [UpdatedAt] DESC;";

            await using var conn = _sqlServer.CreateConnection();
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@BoundOrderID", worksheetNumber);
            await using var reader = await cmd.ExecuteReaderAsync(ct);

            if (!await reader.ReadAsync(ct))
                return null;

            return MapWorkpiece(reader);
        }

        public Task<List<WpTimelineDto>?> GetWorkpieceTimelineByWorkpieceIdAsync(string workpieceId, CancellationToken ct = default)
            => Task.FromResult<List<WpTimelineDto>?>(new List<WpTimelineDto>());

        public async Task<bool> UpdateWorkpieceDataAsync(WorkpieceDto payload, CancellationToken ct = default)
        {
            const string sql = @"
UPDATE [dbo].[WorkpieceTracking]
SET [PartNo] = @PartNo,
    [BoundOrderID] = @BoundOrderID,
    [CurrentStatus] = @CurrentStatus,
    [CurrentLocation] = @CurrentLocation,
    [MeasuredDataJson] = @MeasuredDataJson,
    [HomeLocation] = @HomeLocation,
    [UpdatedAt] = SYSUTCDATETIME()
WHERE [WorkpieceTag] = @WorkpieceTag;";

            await using var conn = _sqlServer.CreateConnection();
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PartNo", payload.workpieceName);
            cmd.Parameters.AddWithValue("@BoundOrderID", payload.worksheetNumber);
            cmd.Parameters.AddWithValue("@CurrentStatus", payload.status);
            cmd.Parameters.AddWithValue("@CurrentLocation", payload.currentLocation);
            cmd.Parameters.AddWithValue("@MeasuredDataJson", payload.restriction ? "{\"restriction\":true}" : "{\"restriction\":false}");
            cmd.Parameters.AddWithValue("@HomeLocation", string.IsNullOrWhiteSpace(payload.tempRetSLocation) ? payload.inspectOffset : payload.tempRetSLocation);
            cmd.Parameters.AddWithValue("@WorkpieceTag", payload.tagSerial);

            return await cmd.ExecuteNonQueryAsync(ct) > 0;
        }

        public async Task<bool> SetWorkpieceRestrictionByTagSerialAsync(string tagSerial, bool restriction, CancellationToken ct = default)
        {
            var dto = await GetWorkpieceByTagSerialAsync(tagSerial, ct);
            if (dto == null)
                return false;

            dto.restriction = restriction;
            return await UpdateWorkpieceDataAsync(dto, ct);
        }

        public async Task<bool> DeleteWorkpieceDataByIdAsync(string id, CancellationToken ct = default)
        {
            const string sql = @"
DELETE FROM [dbo].[WorkpieceTracking]
WHERE [WorkpieceTag] = @WorkpieceTag
   OR [BoundOrderID] = @BoundOrderID;";

            await using var conn = _sqlServer.CreateConnection();
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@WorkpieceTag", id);
            cmd.Parameters.AddWithValue("@BoundOrderID", id);

            return await cmd.ExecuteNonQueryAsync(ct) > 0;
        }

        private static WorkpieceDto MapWorkpiece(SqlDataReader reader)
        {
            return new WorkpieceDto
            {
                _id = reader["WorkpieceTag"]?.ToString() ?? string.Empty,
                tagSerial = reader["WorkpieceTag"]?.ToString() ?? string.Empty,
                workpieceName = reader["PartNo"]?.ToString() ?? string.Empty,
                worksheetNumber = reader["BoundOrderID"]?.ToString() ?? string.Empty,
                status = reader["CurrentStatus"]?.ToString() ?? string.Empty,
                currentLocation = reader["CurrentLocation"]?.ToString() ?? string.Empty,
                restriction = (reader["MeasuredDataJson"]?.ToString() ?? string.Empty).Contains("\"restriction\":true", StringComparison.OrdinalIgnoreCase),
                tempRetSLocation = reader["HomeLocation"]?.ToString() ?? string.Empty,
                inspectOffset = reader["HomeLocation"]?.ToString() ?? string.Empty
            };
        }
    }
}
