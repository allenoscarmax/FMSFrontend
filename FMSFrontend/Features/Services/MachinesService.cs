using FMSFrontend.Features.Dtos;
using FMSFrontend.Services;
using System.Threading;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Services
{
    namespace FMSFrontend.Features.Services
    {
        public interface IMachinesService
        {
            // ==== Repository (DB_*) ====
            Task<List<MachinesDto>?> GetAllMachinesAsync(CancellationToken ct = default);
            Task<List<MachinesDto>?> GetMachinesByMachineNameAsync(string machineName, CancellationToken ct = default);
            Task<string?> GetMachineStatusByIdAsync(string id, CancellationToken ct = default);
            Task<bool> UpdateMachinesDataAsync(MachinesDto machinesDto, CancellationToken ct = default);
            Task<bool> SetMachineStatusByIdAsync(string id, string status, CancellationToken ct = default);

            // OnDeck 綁定
            Task<bool> SetOnDeckElectrodeSerialAsync(string id, string onDeckObjSerial, CancellationToken ct = default);
            Task<bool> SetOnDeckWorksheetSerialAsync(string id, string onDeckObjSerial, CancellationToken ct = default);
            Task<bool> SetOnDeckWorkpieceSerialAsync(string id, string onDeckObjSerial, CancellationToken ct = default);

            // ==== Function ====
            Task<bool> AllMachineConnectAsync(CancellationToken ct = default);
            Task<bool> ConnectAsync(int no, CancellationToken ct = default);
            Task<bool> DisconnectAsync(int no, CancellationToken ct = default);
            Task<bool> SetMachineCanControlAsync(int no, bool canControl, CancellationToken ct = default);
            Task<OscarmaxMachineParaDto?> GetMachineDataAsync(int no, CancellationToken ct = default);
            Task<int> GetMachineCountAsync(CancellationToken ct = default);
            Task<bool> ResetDispatchErrorMessageAsync(int no, CancellationToken ct = default);
        }

        public class MachinesService : IMachinesService
        {
            private readonly IHttpService _http;
            public MachinesService(IHttpService http) => _http = http;

            // ==== Repository (DB_*) ====

            public async Task<List<MachinesDto>?> GetAllMachinesAsync(CancellationToken ct = default)
                => await _http.GetJsonAsync<List<MachinesDto>>("Machine/DB_GetAllMachines", ct);

            public async Task<List<MachinesDto>?> GetMachinesByMachineNameAsync(string machineName, CancellationToken ct = default)
                => await _http.GetJsonAsync<List<MachinesDto>>($"Machine/DB_GetMachinesByMachineName/{Uri.EscapeDataString(machineName)}", ct);

            public async Task<string?> GetMachineStatusByIdAsync(string id, CancellationToken ct = default)
                => await _http.GetJsonAsync<string>($"Machine/DB_GetMachineStatusbyId/{Uri.EscapeDataString(id)}", ct);

            public async Task<bool> UpdateMachinesDataAsync(MachinesDto machinesDto, CancellationToken ct = default)
                => await _http.SendPutAsync("Machine/DB_UpdateMachineData", machinesDto);

            public async Task<bool> SetMachineStatusByIdAsync(string id, string status, CancellationToken ct = default)
                => await _http.SendPutAsync($"Machine/DB_SetMachineStatusbyId/{Uri.EscapeDataString(id)}/{Uri.EscapeDataString(status)}", new { });

            public async Task<bool> SetOnDeckElectrodeSerialAsync(string id, string onDeckObjSerial, CancellationToken ct = default)
                => await _http.SendPutAsync($"Machine/DB_SetMachineOnDeckElectrodeSerialbyId/{Uri.EscapeDataString(id)}?OnDeckObjSerial={Uri.EscapeDataString(onDeckObjSerial)}", new { });

            public async Task<bool> SetOnDeckWorksheetSerialAsync(string id, string onDeckObjSerial, CancellationToken ct = default)
                => await _http.SendPutAsync($"Machine/DB_SetMachineOnDeckWorksheetSerialbyId/{Uri.EscapeDataString(id)}?OnDeckObjSerial={Uri.EscapeDataString(onDeckObjSerial)}", new { });

            public async Task<bool> SetOnDeckWorkpieceSerialAsync(string id, string onDeckObjSerial, CancellationToken ct = default)
                => await _http.SendPutAsync($"Machine/DB_SetMachineOnDeckWorkpieceSerialbyId/{Uri.EscapeDataString(id)}?OnDeckObjSerial={Uri.EscapeDataString(onDeckObjSerial)}", new { });

            // ==== Function ====

            public async Task<bool> AllMachineConnectAsync(CancellationToken ct = default)
                => await _http.GetJsonAsync<bool>("Machine/AllMachineConnect", ct);

            public async Task<bool> ConnectAsync(int no, CancellationToken ct = default)
                => await _http.GetJsonAsync<bool>($"Machine/Connect/{no}", ct);

            public async Task<bool> DisconnectAsync(int no, CancellationToken ct = default)
                => await _http.GetJsonAsync<bool>($"Machine/Disconnect/{no}", ct);

            public async Task<bool> SetMachineCanControlAsync(int no, bool canControl, CancellationToken ct = default)
                => await _http.SendPutAsync($"Machine/SetMachineCanControl/{no}/{canControl}", new { });

            public async Task<OscarmaxMachineParaDto?> GetMachineDataAsync(int no, CancellationToken ct = default)
                => await _http.GetJsonAsync<OscarmaxMachineParaDto>($"Machine/GetMachineData/{no}", ct);

            public async Task<int> GetMachineCountAsync(CancellationToken ct = default)
                => await _http.GetJsonAsync<int>("Machine/GetMachineCount", ct);

            public async Task<bool> ResetDispatchErrorMessageAsync(int no, CancellationToken ct = default)
                => await _http.SendPutAsync($"Machine/ResetDispathErrorMes/{no}", new { });
        }
    }

}
