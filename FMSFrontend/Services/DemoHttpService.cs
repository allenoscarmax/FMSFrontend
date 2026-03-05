using FMSFrontend.Features.Dtos.Database;
using System.Globalization;
using System.Text.Json;
using FMSFrontend.Features.Dtos;

namespace FMSFrontend.Services
{
    public class DemoHttpService : IHttpService
    {
        private readonly object _sync = new();
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly List<ErrorMessageLogDto> _alarmLogs;
        private readonly List<MachinesDto> _machines;
        private readonly List<StorageDto> _storages;
        private readonly List<ElectrodeDto> _electrodes;
        private readonly List<WorkpieceDto> _workpieces;
        private readonly List<WorksheetsDto> _worksheets;
        private readonly List<WorkerDto> _workers;

        public DemoHttpService()
        {
            var now = DateTime.Now;
            _alarmLogs =
            [
                new ErrorMessageLogDto
                {
                    Id = Guid.NewGuid().ToString("N"),
                    TimeStamp = now.AddMinutes(-2),
                    ErrorCode = "E101",
                    MessageCn = "機台門未關閉",
                    MessageEn = "Machine door is open",
                    Note = "Demo",
                    WhichLine = "Line-A",
                    IsAverted = false
                },
                new ErrorMessageLogDto
                {
                    Id = Guid.NewGuid().ToString("N"),
                    TimeStamp = now.AddMinutes(-10),
                    ErrorCode = "E205",
                    MessageCn = "氣壓不足",
                    MessageEn = "Air pressure is low",
                    Note = "Demo",
                    WhichLine = "Line-B",
                    IsAverted = false
                },
                new ErrorMessageLogDto
                {
                    Id = Guid.NewGuid().ToString("N"),
                    TimeStamp = now.AddHours(-1),
                    ErrorCode = "E001",
                    MessageCn = "系統已復歸",
                    MessageEn = "System reset",
                    Note = "Demo",
                    WhichLine = "Line-A",
                    IsAverted = true
                }
            ];

            _workers =
            [
                new WorkerDto
                {
                    Id = Guid.NewGuid().ToString("N"),
                    WorkerNumber = "admin",
                    WorkerName = "admin",
                    AccountGroup = "Expert",
                    AccountName = "admin",
                    Password = "admin"
                }
            ];

            _worksheets =
            [
                new WorksheetsDto
                {
                    _id = Guid.NewGuid().ToString("N"),
                    worksheetNumber = "20260304000000",
                    workpieceName = "EX_1-W",
                    workPriority = 1,
                    workEnabled = true,
                    workStatus = "Queue",
                    workPercentage = "10",
                    targetEDM = "EDM101",
                    processStep = 1,
                    totalProcessStep = 3,
                    coordinate = "A1",
                    setupUser = "admin"
                },
            ];

            _electrodes =
            [
                new ElectrodeDto
                {
                    _id = "6969aafcbf1189f0903bd9ef",
                    worksheetNumber = "20260304000000",
                    worksheetDone = "",
                    tagSerial = "111",
                    electrodeName = "EX_1-001A-01",
                    electrodeType = "Small",
                    currentLocation = "",
                    state = "Verified",
                    pairedEDM = "",
                    offsetStatus = 2,
                    offset = "",
                    complementUpload = false,
                    lifeTimes = 1,
                    useTimes = 0,
                    underSize = "",
                    restriction = false,
                    edmpgm = "EX_1",
                    edM_offsetPGM = "",
                    measuremented = false,
                    measurementStatus = "",
                    shared = false,
                    shareLink = "",
                    tempRetSLocation = "",
                    setupUser = "admin"
                },
                new ElectrodeDto
                {
                    _id = Guid.NewGuid().ToString("N"),
                    tagSerial = "112",
                    electrodeName = "EX_1-001A-02",
                    state = "Verified",
                    currentLocation = "inStore",
                    worksheetNumber = "20260304000000",
                    edmpgm = "EX_1",
                    restriction = false,
                    setupUser = "System"
                },
                new ElectrodeDto
                {
                    _id = Guid.NewGuid().ToString("N"),
                    tagSerial = "113",
                    electrodeName = "EX_1-001A-03",
                    state = "Verified",
                    currentLocation = "inStore",
                    worksheetNumber = "20260304000000",
                    edmpgm = "EX_1",
                    restriction = false,
                    setupUser = "System"
                },
                new ElectrodeDto
                {
                    _id = Guid.NewGuid().ToString("N"),
                    tagSerial = "114",
                    electrodeName = "EX_1-001A-04",
                    state = "Verified",
                    currentLocation = "inStore",
                    worksheetNumber = "20260304000000",
                    edmpgm = "EX_1",
                    restriction = false,
                    setupUser = "System"
                },
            ];

            _workpieces =
            [
                new WorkpieceDto
                {
                    _id = Guid.NewGuid().ToString("N"),
                    tagSerial = "222",
                    workpieceName = "EX_1-W",
                    worksheetNumber = "20260304000000",
                    status = "Verified",
                    restriction = false,
                    currentLocation = "inStore",
                    edmpgm = "EX_1",
                    setupUser = "System"
                }
            ];

            _storages = [];

            // 電極庫 E-1：5 x 10（0-based：row 0~4, col 0~9）
            // row=0、col=0~3 => ondeskTagserial 111~114
            for (var row = 0; row < 5; row++)
            {
                for (var column = 0; column < 10; column++)
                {
                    var serial = row == 1 && column >=1 && column <= 4
                        ? "11"+(column).ToString()
                        : "";

                    _storages.Add(new StorageDto
                    {
                        _id = Guid.NewGuid().ToString("N"),
                        storageName = "E",
                        storageNumber = "1",
                        region = 1,
                        column = column,
                        row = row,
                        ondeskTagserial = serial,
                        state = "Vacant",
                        restriction = false,
                        note = "Demo"
                    });
                }
            }

            // 工件庫 W-1：2 x 5，第一個工件序號 222
            for (var row = 1; row <= 2; row++)
            {
                for (var column = 1; column <= 5; column++)
                {
                    _storages.Add(new StorageDto
                    {
                        _id = Guid.NewGuid().ToString("N"),
                        storageName = "W",
                        storageNumber = "1",
                        region = 1,
                        column = column,
                        row = row,
                        ondeskTagserial = row == 1 && column == 1 ? "222" : "",
                        state = "Vacant",
                        restriction = false,
                        note = "Demo"
                    });
                }
            }

            _machines =
            [
                new MachinesDto
                {
                    _id = "695f46d02a76f769bf4d1b0d",
                    machineCode = "EDM101",
                    productionLine = 1,
                    machineNumber = 1,
                    machineName = "EDM101",
                    ip = "192.168.10.10",
                    port = "13101",
                    remotePassword = "1758",
                    remotePath = "",
                    status = "Disconnection",
                    onDeckElectrodeSerial = "",
                    onDeckWorkpieceSerial = "",
                    onDeckWorksheetSerial = "",
                    setupUser = "System"
                },
                new MachinesDto
                {
                    _id = "695f46d02a76f769bf4d1b0e",
                    machineCode = "EDM102",
                    productionLine = 1,
                    machineNumber = 2,
                    machineName = "EDM102",
                    ip = "192.168.10.101",
                    port = "13102",
                    remotePassword = "1758",
                    remotePath = "",
                    status = "Disconnection",
                    onDeckElectrodeSerial = "2",
                    onDeckWorkpieceSerial = "12",
                    onDeckWorksheetSerial = "",
                    setupUser = "System"
                }
            ];

            ServerIp = "demo";
        }

        public string? ServerIp { get; private set; }

        public Task SendGetAsync(string route) => Task.CompletedTask;

        public Task SendPostAsync<T>(string route, T payload) => Task.CompletedTask;

        public Task<bool> SendPutAsync<T>(string route, T payload)
        {
            var normalized = NormalizeRoute(route);

            if (normalized.StartsWith("Alarm/DB_RemoveErrorMessageLogByDateTime/", StringComparison.OrdinalIgnoreCase))
            {
                if (TryParseDateRange(normalized, "Alarm/DB_RemoveErrorMessageLogByDateTime", out var start, out var end))
                {
                    lock (_sync)
                    {
                        _alarmLogs.RemoveAll(x => x.TimeStamp >= start && x.TimeStamp <= end);
                    }
                }

                return Task.FromResult(true);
            }

            if (normalized.Equals("Alarm/DB_DeleteAllErrorMessageData", StringComparison.OrdinalIgnoreCase))
            {
                lock (_sync)
                {
                    _alarmLogs.Clear();
                }

                return Task.FromResult(true);
            }

            return Task.FromResult(true);
        }

        public Task SendDeleteAsync(string route) => Task.CompletedTask;

        public Task<T?> GetJsonAsync<T>(string route, CancellationToken cancellationToken = default)
        {
            var normalized = NormalizeRoute(route);

            if (normalized.Equals("Health/CheckHealth", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(ConvertTo<T>(true));

            if (normalized.Equals("Machine/DB_GetAllMachines", StringComparison.OrdinalIgnoreCase))
            {
                List<MachinesDto> result;
                lock (_sync)
                {
                    result = _machines
                        .OrderBy(x => x.productionLine)
                        .ThenBy(x => x.machineNumber)
                        .ToList();
                }

                return Task.FromResult(ConvertTo<T>(result));
            }

            if (normalized.Equals("Worker/DB_GetAllWorker", StringComparison.OrdinalIgnoreCase))
            {
                List<WorkerDto> result;
                lock (_sync)
                {
                    result = _workers.ToList();
                }

                return Task.FromResult(ConvertTo<T>(result));
            }

            if (normalized.StartsWith("Electrode/DB_GetElectrodeByWorksheetNumber/", StringComparison.OrdinalIgnoreCase))
            {
                List<ElectrodeDto> result;
                lock (_sync)
                {
                    result = _electrodes
                        .Where(x => x.tagSerial is "111" or "112" or "113" or "114")
                        .ToList();
                }

                return Task.FromResult(ConvertTo<T>(result));
            }

            if (normalized.StartsWith("Worksheet/DB_GetWorkSheetByWorkStatus/", StringComparison.OrdinalIgnoreCase))
            {
                var status = GetLastSegment(normalized);
                if (string.IsNullOrWhiteSpace(status))
                    return Task.FromResult(default(T));

                List<WorksheetsDto> result;
                lock (_sync)
                {
                    result = _worksheets
                        .Take(2)
                        .Select(x => new WorksheetsDto
                        {
                            _id = x._id,
                            worksheetNumber = x.worksheetNumber,
                            workpieceName = x.workpieceName,
                            workPriority = x.workPriority,
                            workEnabled = x.workEnabled,
                            workStatus = status,
                            workPercentage = x.workPercentage,
                            targetEDM = x.targetEDM,
                            processStep = x.processStep,
                            totalProcessStep = x.totalProcessStep,
                            coordinate = x.coordinate,
                            setupUser = x.setupUser
                        })
                        .ToList();
                }

                return Task.FromResult(ConvertTo<T>(result));
            }

            if (normalized.StartsWith("Electrode/DB_GetElectrodesbyTagSerial/", StringComparison.OrdinalIgnoreCase))
            {
                var tagSerial = GetLastSegment(normalized);
                if (string.IsNullOrWhiteSpace(tagSerial))
                    return Task.FromResult(default(T));

                List<ElectrodeDto> result;
                lock (_sync)
                {
                    result = _electrodes
                        .Where(x => string.Equals(x.tagSerial, tagSerial, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                return Task.FromResult(ConvertTo<T>(result));
            }

            if (normalized.StartsWith("Workpiece/DB_GetWorkpieceByTagSerial/", StringComparison.OrdinalIgnoreCase))
            {
                var tagSerial = GetLastSegment(normalized);
                if (string.IsNullOrWhiteSpace(tagSerial))
                    return Task.FromResult(default(T));

                WorkpieceDto? result;
                lock (_sync)
                {
                    result = _workpieces
                        .FirstOrDefault(x => string.Equals(x.tagSerial, tagSerial, StringComparison.OrdinalIgnoreCase));
                }

                if (result is null)
                    return Task.FromResult(default(T));

                return Task.FromResult(ConvertTo<T>(result));
            }

            if (normalized.Equals("Storage/DB_GetAllStorageData", StringComparison.OrdinalIgnoreCase))
            {
                List<StorageDto> result;
                lock (_sync)
                {
                    result = _storages
                        .OrderBy(x => x.storageName)
                        .ThenBy(x => x.storageNumber)
                        .ThenBy(x => x.region)
                        .ThenBy(x => x.row)
                        .ThenBy(x => x.column)
                        .ToList();
                }

                return Task.FromResult(ConvertTo<T>(result));
            }

            if (normalized.Equals("Alarm/DB_GetCurrentErrorMessageLog", StringComparison.OrdinalIgnoreCase))
            {
                List<ErrorMessageLogDto> result;
                lock (_sync)
                {
                    result = _alarmLogs
                        .Where(x => !x.IsAverted)
                        .OrderByDescending(x => x.TimeStamp)
                        .ToList();
                }

                return Task.FromResult(ConvertTo<T>(result));
            }

            if (normalized.StartsWith("Alarm/DB_GetErrorMessageLogByDateTime/", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryParseDateRange(normalized, "Alarm/DB_GetErrorMessageLogByDateTime", out var start, out var end))
                    return Task.FromResult(default(T));

                List<ErrorMessageLogDto> result;
                lock (_sync)
                {
                    result = _alarmLogs
                        .Where(x => x.TimeStamp >= start && x.TimeStamp <= end)
                        .OrderByDescending(x => x.TimeStamp)
                        .ToList();
                }

                return Task.FromResult(ConvertTo<T>(result));
            }

            return Task.FromResult(default(T));
        }

        public async Task<string?> GetJsonAsyncNoDeserialize(string route, CancellationToken cancellationToken = default)
        {
            var data = await GetJsonAsync<object>(route, cancellationToken);
            if (data is null)
                return null;

            return JsonSerializer.Serialize(data, _jsonOptions);
        }

        public Task<TResult?> PutJsonAsync<TRequest, TResult>(string route, TRequest payload, CancellationToken cancellationToken = default)
            => Task.FromResult(default(TResult));

        public void UpdateServerIp(string ip)
        {
            ServerIp = ip;
        }

        private static string NormalizeRoute(string route)
        {
            var value = (route ?? string.Empty).Trim();

            if (Uri.TryCreate(value, UriKind.Absolute, out var absoluteUri))
            {
                value = absoluteUri.PathAndQuery;
            }

            return value.TrimStart('/');
        }

        private static bool TryParseDateRange(string route, string prefix, out DateTime start, out DateTime end)
        {
            start = default;
            end = default;

            var parts = route.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4)
                return false;

            if (!parts[0].Equals("Alarm", StringComparison.OrdinalIgnoreCase))
                return false;

            if (!parts[1].Equals(prefix.Split('/')[1], StringComparison.OrdinalIgnoreCase))
                return false;

            var startRaw = Uri.UnescapeDataString(parts[2]);
            var endRaw = Uri.UnescapeDataString(parts[3]);

            var okStart = DateTime.TryParse(startRaw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out start);
            var okEnd = DateTime.TryParse(endRaw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out end);
            return okStart && okEnd;
        }

        private static string GetLastSegment(string route)
        {
            var path = route.Split('?', 2)[0];
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return string.Empty;

            return Uri.UnescapeDataString(parts[^1]);
        }

        private T? ConvertTo<T>(object value)
        {
            if (value is T typed)
                return typed;

            try
            {
                var json = JsonSerializer.Serialize(value, _jsonOptions);
                return JsonSerializer.Deserialize<T>(json, _jsonOptions);
            }
            catch
            {
                return default;
            }
        }
    }
}
