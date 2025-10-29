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
using System.Linq;
using System.Windows.Threading;

namespace FMSFrontend.ViewModels.Production
{
    public partial class MachineDetailViewModel : ObservableObject
    {
        private readonly ProductionLinesViewModel _parent;
        private readonly IHttpService _httpService;
        public Task RefreshAsync() => Task.Run(async () => await UpdateMachinesAsync());
        public MachineDetailViewModel(ProductionLinesViewModel parent, IHttpService httpService)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));

            // 非同步初始化：不要在建構子 await，改以 fire-and-forget 並記錄例外
            _ = LoadMachinesAsync();
        }

        // Make the collection settable so we can replace it in one UI operation to avoid per-item layout churn
        private ObservableCollection<MachineCardViewModel> _machineDetails = new();
        public ObservableCollection<MachineCardViewModel> MachineDetails
        {
            get => _machineDetails;
            private set => SetProperty(ref _machineDetails, value);
        }

        private async Task LoadMachinesAsync()
        {
            try
            {
                // 1) 取得資料（在背景執行）
                var machines = await _httpService.GetJsonAsync<List<Machines>>("Machine/DB_GetAllMachines", default) ?? new List<Machines>();

                // 2) 在背景建立要綁定的 view models（避免在 UI 執行緒建立大量物件）
                var cards = machines.Select(m => MapToWorkOrderData(m)).ToList();

                // 3) 在 UI 執行緒一次性設定 delegate 並替換集合（使用低優先順序以讓 UI 互動先行）
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var card in cards)
                    {
                        card.OpenWorkpieceInfo = (wp, tl) => _parent._windowService.ShowMaterialInformation(wp, tl);
                        card.OpenElectrodeInfo = (el, tl) => _parent._windowService.ShowMaterialInformation(el, tl);
                    }

                    // Replace the whole collection in one operation to minimize layout/measure passes
                    MachineDetails = new ObservableCollection<MachineCardViewModel>(cards);
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
                // 直接用強型別取得 List<Machines>（在背景執行）
                var machines = await _httpService.GetJsonAsync<List<Machines>>("Machine/DB_GetAllMachines", default) ?? new List<Machines>();

                // 在 UI 執行緒更新集合（使用非同步 Invoke，且較低優先順序）
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    // 建立快速搜尋表
                    var fetchedByName = machines
                        .Where(m => !string.IsNullOrWhiteSpace(m.machineName))
                        .ToDictionary(m => m.machineName!, StringComparer.OrdinalIgnoreCase);

                    // 記錄已處理的機台名稱
                    var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    // 更新現有項目或移除已不存在的機台
                    foreach (var existing in MachineDetails.ToList())
                    {
                        if (existing == null) continue;

                        var name = existing.MachineName ?? string.Empty;
                        if (fetchedByName.TryGetValue(name, out var m))
                        {
                            // 更新欄位（只更新可能變動的欄位）
                            existing.Status = m.status;
                            existing.Type = MapToMachineType(m.MachineCode?.ToString() ?? "");

                            processed.Add(name);
                        }
                        else
                        {
                            // 若伺服器回傳已不包含該機台，從集合移除
                            MachineDetails.Remove(existing);
                        }
                    }

                    // 新增伺服器有但集合裡沒有的機台
                    foreach (var m in machines)
                    {
                        var name = m.machineName ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(name)) continue;
                        if (processed.Contains(name)) continue;

                        var card = MapToWorkOrderData(m);
                        card.OpenWorkpieceInfo = (wp, tl) => _parent._windowService.ShowMaterialInformation(wp, tl);
                        card.OpenElectrodeInfo = (el, tl) => _parent._windowService.ShowMaterialInformation(el, tl);
                        MachineDetails.Add(card);
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

    }
}
