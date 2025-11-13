using FMSFrontend.Features.Dtos;
using FMSFrontend.Services;
using OSCARMAXFMS_V3.DBmodels;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FMSFrontend.Features.Services
{
    public interface IElectrodeService
    {
        // ===== Electrode Repository =====
        Task<bool> DB_InsertElectrodeAsync(ElectrodeDto dto, CancellationToken ct = default);
        Task<bool> DB_UpdateElectrodeDataAsync(ElectrodeDto dto, CancellationToken ct = default);

        Task<List<ElectrodeDto>?> DB_GetAllElectrodeAsync(CancellationToken ct = default);
        Task<List<ElectrodeDto>?> DB_GetAllOnShelfElectrodeAsync(CancellationToken ct = default);
        Task<List<ElectrodeDto>?> DB_GetElectrodeBySpecificStateAsync(string state, CancellationToken ct = default);
        Task<List<ElectrodeDto>?> DB_GetElectrodesByTagSerialAsync(string tagSerial, CancellationToken ct = default);
        Task<ElectrodeDto?> DB_GetElectrodeByIdAsync(string id, CancellationToken ct = default);
        Task<List<ElectrodeDto>?> GetElectrodeByWorksheetNumberAsync(string WorksheetNumber, CancellationToken ct = default);

        Task<bool> DB_SetElectrodeStateByTagSerialAsync(string tagSerial, string state, CancellationToken ct = default);
        Task<bool> DB_SetElectrodeUseTimesByTagSerialAsync(string tagSerial, int useTimes, CancellationToken ct = default);
        Task<bool> DB_SetElectrodeRestrictionByTagSerialAsync(string tagSerial, bool restriction, CancellationToken ct = default);
        Task<bool> DB_SetElectrodeOffsetByTagSerialAsync(string tagSerial, string offset, CancellationToken ct = default);

        Task<bool> DB_SetElectrodeStateByIdAsync(string id, string state, CancellationToken ct = default);
        Task<bool> DB_SetElectrodeUseTimesByIdAsync(string id, int useTimes, CancellationToken ct = default);
        Task<bool> DB_SetElectrodeRestrictionByIdAsync(string id, bool restriction, CancellationToken ct = default);
        Task<bool> DB_SetElectrodeOffsetByIdAsync(string id, string offset, CancellationToken ct = default); // 後端路由存在，內部實作可能有小 typo
        Task<bool> DB_SetElectrodeCurrentLocationByIdAsync(string id, string currentLocation, CancellationToken ct = default);

        Task<bool> DB_DeleteAllElectrodeDataAsync(CancellationToken ct = default);
        Task<bool> DB_DeleteElectrodeDataByIdAsync(string id, CancellationToken ct = default);
        Task<bool> DB_RemoveElectrodeTagSerialDataByIdAsync(string id, CancellationToken ct = default);

        // ===== Electrode Timeline =====
        Task<bool> DB_InsertNewElectrodeTimelineAsync(EleTimelineDto dto, CancellationToken ct = default);
        Task<List<EleTimelineDto>?> DB_GetElectrodeTimelineByIdAsync(string id, CancellationToken ct = default);
        Task<List<EleTimelineDto>?> DB_GetElectrodeTimelineByElectrodeIdAsync(string electrodeId, CancellationToken ct = default);
        Task<List<EleTimelineDto>?> DB_GetElectrodeTimelineByDateTimeAsync(DateTime start, DateTime end, CancellationToken ct = default);
        Task<bool> DB_DeleteAllElectrodeTimelineAsync(CancellationToken ct = default);
    }

    public class ElectrodeService : IElectrodeService
    {
        private readonly IHttpService _http;
        public ElectrodeService(IHttpService http) => _http = http;

        private static string Enc(string s) => Uri.EscapeDataString(s ?? string.Empty);
        private static string Iso(DateTime dt) => Uri.EscapeDataString(dt.ToString("o", CultureInfo.InvariantCulture));

        // ===== Electrode Repository =====

        public async Task<bool> DB_InsertElectrodeAsync(ElectrodeDto dto, CancellationToken ct = default)
            => await _http.SendPutAsync("Electrode/DB_InsertElectrode", dto);

        public async Task<bool> DB_UpdateElectrodeDataAsync(ElectrodeDto dto, CancellationToken ct = default)
            => await _http.SendPutAsync("Electrode/DB_UpdateElectrodeData", dto);

        public async Task<List<ElectrodeDto>?> DB_GetAllElectrodeAsync(CancellationToken ct = default)
            => await _http.GetJsonAsync<List<ElectrodeDto>>("Electrode/DB_GetAllElectrode", ct);

        public async Task<List<ElectrodeDto>?> DB_GetAllOnShelfElectrodeAsync(CancellationToken ct = default)
            => await _http.GetJsonAsync<List<ElectrodeDto>>("Electrode/DB_GetAllOnShelfElectrode", ct);

        public async Task<List<ElectrodeDto>?> DB_GetElectrodeBySpecificStateAsync(string state, CancellationToken ct = default)
            => await _http.GetJsonAsync<List<ElectrodeDto>>($"Electrode/DB_GetElectrodeBySpecificState/{Enc(state)}", ct);

        public async Task<List<ElectrodeDto>?> DB_GetElectrodesByTagSerialAsync(string tagSerial, CancellationToken ct = default)
            => await _http.GetJsonAsync<List<ElectrodeDto>>($"Electrode/DB_GetElectrodesbyTagSerial/{Enc(tagSerial)}", ct);

        public async Task<ElectrodeDto?> DB_GetElectrodeByIdAsync(string id, CancellationToken ct = default)
            => await _http.GetJsonAsync<ElectrodeDto>($"Electrode/DB_GetElectrodeById/{Enc(id)}", ct);
        public async Task<List<ElectrodeDto>?> GetElectrodeByWorksheetNumberAsync(string WorksheetNumber, CancellationToken ct = default)
            => await _http.GetJsonAsync<List<ElectrodeDto>>($"Electrode/DB_GetElectrodeByWorksheetNumber/{Enc(WorksheetNumber)}", ct);
        public async Task<bool> DB_SetElectrodeStateByTagSerialAsync(string tagSerial, string state, CancellationToken ct = default)
            => await _http.SendPutAsync($"Electrode/DB_SetElectrodeStatebyTagSerial/{Enc(tagSerial)}/{Enc(state)}", new { });

        public async Task<bool> DB_SetElectrodeUseTimesByTagSerialAsync(string tagSerial, int useTimes, CancellationToken ct = default)
            => await _http.SendPutAsync($"Electrode/DB_SetElectrodeUseTimesbyTagSerial/{Enc(tagSerial)}/{useTimes}", new { });

        public async Task<bool> DB_SetElectrodeRestrictionByTagSerialAsync(string tagSerial, bool restriction, CancellationToken ct = default)
            => await _http.SendPutAsync($"Electrode/DB_SetElectrodeRestrictionbyTagSerial/{Enc(tagSerial)}/{restriction.ToString().ToLower()}", new { });

        public async Task<bool> DB_SetElectrodeOffsetByTagSerialAsync(string tagSerial, string offset, CancellationToken ct = default)
            => await _http.SendPutAsync($"Electrode/DB_SetElectrodeOffsetbyTagSerial/{Enc(tagSerial)}/{Enc(offset)}", new { });

        public async Task<bool> DB_SetElectrodeStateByIdAsync(string id, string state, CancellationToken ct = default)
            => await _http.SendPutAsync($"Electrode/DB_SetElectrodeStatebyId/{Enc(id)}/{Enc(state)}", new { });

        public async Task<bool> DB_SetElectrodeUseTimesByIdAsync(string id, int useTimes, CancellationToken ct = default)
            => await _http.SendPutAsync($"Electrode/DB_SetElectrodeUseTimesbyId/{Enc(id)}/{useTimes}", new { });

        public async Task<bool> DB_SetElectrodeRestrictionByIdAsync(string id, bool restriction, CancellationToken ct = default)
            => await _http.SendPutAsync($"Electrode/DB_SetElectrodeRestrictionbyId/{Enc(id)}/{restriction.ToString().ToLower()}", new { });

        // 注意：後端此路由存在，但你貼的控制器內部呼叫似乎用到了 TagSerial 版的函式；先照路由實作呼叫，後端若改名即可相容。
        public async Task<bool> DB_SetElectrodeOffsetByIdAsync(string id, string offset, CancellationToken ct = default)
            => await _http.SendPutAsync($"Electrode/DB_SetElectrodeOffsetbyId/{Enc(id)}/{Enc(offset)}", new { });

        public async Task<bool> DB_SetElectrodeCurrentLocationByIdAsync(string id, string currentLocation, CancellationToken ct = default)
            => await _http.SendPutAsync($"Electrode/DB_SetElectrodeCurrentLocationbyId/{Enc(id)}/{Enc(currentLocation)}", new { });

        public async Task<bool> DB_DeleteAllElectrodeDataAsync(CancellationToken ct = default)
            => await _http.SendPutAsync("Electrode/DB_DeleteAllElectrodeData", new { });

        public async Task<bool> DB_DeleteElectrodeDataByIdAsync(string id, CancellationToken ct = default)
            => await _http.SendPutAsync($"Electrode/DB_DeleteElectrodeDataById/{Enc(id)}", new { });

        public async Task<bool> DB_RemoveElectrodeTagSerialDataByIdAsync(string id, CancellationToken ct = default)
            => await _http.SendPutAsync($"Electrode/DB_RemoveElectrodeTagSerialDatabyId/{Enc(id)}", new { });

        // ===== Electrode Timeline =====

        public async Task<bool> DB_InsertNewElectrodeTimelineAsync(EleTimelineDto dto, CancellationToken ct = default)
            => await _http.SendPutAsync("Electrode/DB_InsertNewElectrodeTimeline", dto);

        public async Task<List<EleTimelineDto>?> DB_GetElectrodeTimelineByIdAsync(string id, CancellationToken ct = default)
            => await _http.GetJsonAsync<List<EleTimelineDto>>($"Electrode/DB_GetElectrodeTimelineById/{Enc(id)}", ct);

        public async Task<List<EleTimelineDto>?> DB_GetElectrodeTimelineByElectrodeIdAsync(string electrodeId, CancellationToken ct = default)
            => await _http.GetJsonAsync<List<EleTimelineDto>>($"Electrode/DB_GetElectrodeTimelineByElectrodeId/{Enc(electrodeId)}", ct);

        public async Task<List<EleTimelineDto>?> DB_GetElectrodeTimelineByDateTimeAsync(DateTime start, DateTime end, CancellationToken ct = default)
            => await _http.GetJsonAsync<List<EleTimelineDto>>($"Electrode/DB_GetElectrodeTimelineByDateTime/{Iso(start)}/{Iso(end)}", ct);

        public async Task<bool> DB_DeleteAllElectrodeTimelineAsync(CancellationToken ct = default)
            => await _http.SendPutAsync("Electrode/DB_DeleteAllElectrodeTimeline", new { });
    }
}
