using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Standard;
using FMSFrontend.Interfaces;
using FMSFrontend.Services;
using FMSFrontend.ViewModels.Windows;
using FMSFrontend.Views.Windows;
using OSCARMAXFMS_V3.DBmodels;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using static FMSFrontend.ViewModels.ElectrodeDetailViewModel;
using FMSFrontend.Models;
namespace FMSFrontend.ViewModels.Production
{
    public partial class MachineOverviewViewModel : ObservableObject
    {
        private readonly ProductionLinesViewModel _parent;
        private readonly IHttpService _httpService;
        public Task UpdataAsync() => Task.Run(async () => await UpdateMachinesAsync());
        public Task LoadAsync() => Task.Run(async () => await LoadMachinesAsync());
        public ObservableCollection<MachineCardViewModel> Machines { get; } = new();

        public MachineOverviewViewModel(ProductionLinesViewModel parent, IHttpService httpService)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));

            //_ = LoadMachinesAsync();
            LoadAsync();
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
                // 顯示機器資料
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Machines.Clear();
                    foreach (var m in machines)
                    {
                        var card = MapToWorkOrderData(m);
                        card.OpenWorkpieceInfo = (wp, tl) => _parent._windowService.ShowMaterialInformation(wp, tl, _httpService);
                        card.OpenElectrodeInfo = (el, tl) => _parent._windowService.ShowMaterialInformation(el, tl, _httpService);
                        Machines.Add(card);
                    }
                }, DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                // 簡單紀錄例外；實務可改為紀錄服務或 UI 顯示錯誤
                Debug.WriteLine($"LoadMachinesAsync error: {ex}");
            }
        }
        private async Task UpdateMachinesAsync()
        {
            try
            {
                var machines = await _httpService.GetJsonAsync<List<Machines>>("Machine/DB_GetAllMachines", default) ?? new List<Machines>();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    var fetchedByName = machines
                        .Where(m => !string.IsNullOrWhiteSpace(m.machineName))
                        .ToDictionary(m => m.machineName!, StringComparer.OrdinalIgnoreCase);

                    var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    // Update existing or remove missing
                    foreach (var existing in Machines.ToList())
                    {
                        if (existing == null) continue;
                        var name = existing.MachineName ?? string.Empty;
                        if (fetchedByName.TryGetValue(name, out var m))
                        {
                            existing.Status = m.status;
                            existing.Type = MapToMachineType(m.MachineCode?.ToString() ?? "");
                            processed.Add(name);
                        }
                        else
                        {
                            Machines.Remove(existing);
                        }
                    }

                    // Add new
                    foreach (var m in machines)
                    {
                        var name = m.machineName ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(name)) continue;
                        if (processed.Contains(name)) continue;

                        var card = MapToWorkOrderData(m);
                        card.OpenWorkpieceInfo = (wp, tl) => _parent._windowService.ShowMaterialInformation(wp, tl, _httpService);
                        card.OpenElectrodeInfo = (el, tl) => _parent._windowService.ShowMaterialInformation(el, tl, _httpService);
                        Machines.Add(card);
                    }
                }, DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateMachinesAsync error: {ex}");
            }
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
        private void ToggleExpand()
        {
            // 假設你要展開到特定 StorageId 的 DetailControl
            _parent.ShowMachineDetail();
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
    }

    public partial class MachineCardViewModel : ObservableObject
    {
        [ObservableProperty]
        private string machineName = "EDM-XX";

        [ObservableProperty]
        private string status = "idle"; // 可為 idle / running / warning / error / disabled

        [ObservableProperty]
        private MachineType type = MachineType.EDM;
        [ObservableProperty]
        private bool restriction;

        // ✅ 圖片綁定使用的字串（自動從 enum 轉成檔名）
        public string MachineTypeName => Type.ToString();

        // 由外層注入：用 Model + Timeline 直接開視窗
        public Action<WorkpieceModel, IEnumerable<TimelineItemModel>>? OpenWorkpieceInfo { get; set; }
        public Action<ElectrodeModel, IEnumerable<TimelineItemModel>>? OpenElectrodeInfo { get; set; }


        [RelayCommand]
        private void ShowWorkpieceDetail()
        {
            // 若暫時沒有實際資料，給一筆假資料即可開窗
            var wp = new WorkpieceModel { No = "W-TEST-001", Name = "示範工件" };
            OpenWorkpieceInfo?.Invoke(wp, Array.Empty<TimelineItemModel>());
        }

        [RelayCommand]
        private void ShowElectrodeDetail()
        {
            var elec = new ElectrodeModel { No = "E-TEST-001", Name = "示範電極" };
            OpenElectrodeInfo?.Invoke(elec, Array.Empty<TimelineItemModel>());
        }


    }
    public enum MachineType
    {
        EDM = 0,
        ZNC = 1,
        CNC = 2
    }

}
