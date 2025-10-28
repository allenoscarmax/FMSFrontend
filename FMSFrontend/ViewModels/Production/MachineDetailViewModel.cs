using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Services;
using FMSFrontend.ViewModels.Windows;
using FMSFrontend.Views.Windows;
using OSCARMAXFMS_V3.DBmodels;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using System.Diagnostics;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;
using System.Security.Cryptography;

namespace FMSFrontend.ViewModels.Production
{
    public partial class MachineDetailViewModel : ObservableObject
    {
        private readonly ProductionLinesViewModel _parent;
        private readonly IHttpService _httpService;
        public Task RefreshAsync() => Task.Run(async () => await LoadMachinesAsync());
        public MachineDetailViewModel(ProductionLinesViewModel parent, IHttpService httpService)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));

            // 非同步初始化：不要在建構子 await，改以 fire-and-forget 並記錄例外
            _ = LoadMachinesAsync();
        }

        private async Task LoadMachinesAsync()
        {
            try
            {
                // 讀取 API 回傳的 JSON
                JsonElement? json = await _httpService.GetJsonAsync<JsonElement>("Machine/DB_GetAllMachines", default);
                List<Machines> machines = (json.HasValue && json.Value.ValueKind != JsonValueKind.Undefined)
                    ? JsonSerializer.Deserialize<List<Machines>>(json.Value.GetRawText()) ?? new List<Machines>()
                    : new List<Machines>();

                // 清除舊資料
                MachineDetails.Clear();

                foreach (var m in machines)
                {
                    var card = MapToWorkOrderData(m);

                    // 綁定開啟資訊的動作
                    card.OpenWorkpieceInfo = (wp, tl) => _parent._windowService.ShowMaterialInformation(wp, tl);
                    card.OpenElectrodeInfo = (el, tl) => _parent._windowService.ShowMaterialInformation(el, tl);

                    // 加入集合（在同步上下文中執行，await 會回到 UI 執行緒）
                    MachineDetails.Add(card);
                }
            }
            catch (Exception ex)
            {
                // 簡單紀錄例外；實務可改為紀錄服務或 UI 顯示錯誤
                Debug.WriteLine($"LoadMachinesAsync error: {ex}");
            }
        }
        private async Task UpdateMachinesAsync()
        {
            // 讀取 API 回傳的 JSON
            JsonElement? json = await _httpService.GetJsonAsync<JsonElement>("Machine/DB_GetAllMachines", default);
            List<Machines> machines = (json.HasValue && json.Value.ValueKind != JsonValueKind.Undefined)
                ? JsonSerializer.Deserialize<List<Machines>>(json.Value.GetRawText()) ?? new List<Machines>()
                : new List<Machines>();

        }
        private static MachineCardViewModel MapToWorkOrderData(Machines m)
        {
            return new MachineCardViewModel
            {
                MachineName = m.machineName,
                Status = m.status,
                // 保留既有 Type 欄位並嘗試填寫 MachineTypeName（視 MachineCardViewModel 定義）
                Type = MapToMachineType(m.MachineCode?.ToString() ?? ""),
                Restriction = false
            };
        }
        private static MachineType MapToMachineType(string s)
        {
            return s switch
            {
                "EDM" => MachineType.EDM,
                "CNC" => MachineType.CNC,
                "ZNC" => MachineType.ZNC,
                _ => MachineType.EDM,
            };
        }
        [RelayCommand]
        private void BackToOverview()
        {
            _parent.ShowMachineOverview();
        }

        [RelayCommand]
        private void OpenMachineWindow(object? machine)   // machine 建議是 MachineCardViewModel
        {
            var vm = new ShowMachineWindowViewModel(machine);
            var win = new ShowMachineWindow { DataContext = vm };

            var owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            if (owner != null) win.Owner = owner;

            win.ShowDialog();
        }

        public ObservableCollection<MachineCardViewModel> MachineDetails { get; } = new();
    }
}
