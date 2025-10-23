using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Extensions; // SelectItemWindow / SelectItemType / SelectItem
using FMSFrontend.Interfaces;
using FMSFrontend.ViewModels.Windows;
using FMSFrontend.Views;
using MaterialDesignThemes.Wpf.Transitions;
using OSCARMAXFMS_V3.DBmodels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Threading.Tasks;
using FMSFrontend.Services;
using ControlzEx.Standard;

public partial class MaterialPairViewModel : ObservableObject
{
    private readonly Window _window;
    private readonly IWindowService _windowService;
    private readonly IHttpService _httpService;

    public MaterialPairViewModel(bool isElectrode, Window window, IWindowService windowService, IHttpService httpService)
    {
        ShowElectrodeSection = isElectrode;
        ShowWorkpieceSection = !isElectrode;
        _window = window;
        _windowService = windowService;
        _httpService = httpService;
    }

    // 控制區塊顯示
    [ObservableProperty] private bool showElectrodeSection;
    [ObservableProperty] private bool showWorkpieceSection;

    // 使用者選到的項目（給後續 Pair 或 UI 顯示）
    [ObservableProperty] private SelectItem? selectedElectrodeItem;
    [ObservableProperty] private SelectItem? selectedWorkpieceItem;
    // ★ 將 SelectItem 換成 WorksheetItem
    [ObservableProperty] private WorksheetItem? selectedWorksheetItem;

    [ObservableProperty] private string _lifetime = string.Empty;
    [ObservableProperty] private string _selectStatus = string.Empty;
    [ObservableProperty] private string _tagSerial = string.Empty;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _pgm = string.Empty;
    [ObservableProperty] private string _pairEdm = string.Empty;


    // 方便 UI 綁定顯示文字（可選）
    public string CurrentTitle => ShowElectrodeSection ? "電極配對" : "工件配對";


    // 顯示用屬性（還沒選時有預設文字）
    public string SelectedElectrodeNameDisplay =>
        SelectedElectrodeItem?.MaterialName ?? "請選擇電極";

    public string SelectedWorkpieceNameDisplay =>
        SelectedWorkpieceItem?.MaterialName ?? "請選擇工件";
    // ★ 改用 WorksheetItem 欄位
    public string SelectedWorkOrderName =>
        SelectedWorksheetItem != null
            ? $"{SelectedWorksheetItem.PartName}（{SelectedWorksheetItem.WorkOrderNo}）"
            : "請選擇工單";


    partial void OnSelectedElectrodeItemChanged(SelectItem? value)
    {
        OnPropertyChanged(nameof(SelectedElectrodeNameDisplay));
    }

    partial void OnSelectedWorkpieceItemChanged(SelectItem? value)
    {
        OnPropertyChanged(nameof(SelectedWorkpieceNameDisplay));
    }
    // ★ 選到工單時也要通知 UI 重新取字串
    partial void OnSelectedWorksheetItemChanged(WorksheetItem? value)
    {
        OnPropertyChanged(nameof(SelectedWorkOrderName));
    }

    // Pair
    [RelayCommand]
    private void Pair()
    {
        // TODO: 依你的流程處理配對，例如使用 SelectedElectrodeItem / SelectedWorkpieceItem
        _windowService.ShowMessage($"{CurrentTitle} OK!");
    }

    [RelayCommand]
    private void Back()
    {
        _window?.Close();

        var selectWindow = new MaterialTypeSelectWindow();
        if (selectWindow.ShowDialog() == true)
        {
            var newPairWindow = new MaterialPairWindow(selectWindow.IsElectrodeSelected);
            newPairWindow.ShowDialog();
        }
    }


    // =========================
    // 新增：選擇工件 / 電極
    // =========================
    Brush StatusColor(string s)
    {
        return s switch
        {
            "New" => Brushes.Gold,
            "Verified" => Brushes.Gold,
            "Working" => Brushes.Green,
            "Error" => Brushes.IndianRed,
            "Completed" => Brushes.RoyalBlue,
            "Reserved" => Brushes.Gray,
            "Empty" => Brushes.White,
            _ => Brushes.White
        };
    }

    [RelayCommand]
    private async Task SelectWorkpiece()
    {
        // 1) 準備資料
        // 若你有資料來源，可傳一個 loader 進去；沒有就直接 new
        var items = new List<SelectItem>();
        JsonElement? json = await _httpService.GetJsonAsync<JsonElement>("Workpiece/DB_GetAllWorkpiece", default);
        var Workpiece = json.HasValue ? JsonSerializer.Deserialize<List<Workpiece>>(json.Value.GetRawText()) ?? new List<Workpiece>() : new List<Workpiece>();
        foreach (var wp in Workpiece)
        {
            Brush statusBrush = StatusColor(wp.status);
            // 使用 SelectItem 的建構子（SelectItem 擁有 read-only 屬性與 constructor）
            items.Add(new SelectItem(wp.workpieceName ?? string.Empty, wp.status ?? string.Empty, statusBrush));
        }

        // 2) 建立視窗與 VM
        // SelectItemWindow 的第二個參數是 loader：Func<SelectItemType, IEnumerable<SelectItem>>
        var dlg = new SelectItemWindow(SelectItemType.Workpiece, _ => items)
        {
            Owner = _window
        };

        // 3) 顯示並取回選擇結果
        if (dlg.ShowDialog() == true)
        {
            SelectedWorkpieceItem = dlg.Tag as SelectItem;

            var selectedName = SelectedWorkpieceItem?.MaterialName ?? string.Empty;
            var wp = Workpiece.FirstOrDefault(w => string.Equals(w.workpieceName, selectedName, StringComparison.Ordinal));
            SelectStatus = wp?.status ?? string.Empty;
            TagSerial = wp?.tagSerial ?? string.Empty;
            Name = wp.workpieceName ?? string.Empty;
            PairEdm = wp.pairedEDM ?? string.Empty;
            Pgm = wp.edmpgm ?? string.Empty;
            // 若切換選擇邏輯需要清掉另一邊，可視需求做：
            // SelectedElectrodeItem = null;
        }
    }

    [RelayCommand]
    private async Task SelectElectrode()
    {
        // 1) 準備資料
        var items = new List<SelectItem>();
        JsonElement? json = await _httpService.GetJsonAsync<JsonElement>("Electrode/DB_GetAllElectrode", default);
        var Electrode = json.HasValue ? JsonSerializer.Deserialize<List<Electrode>>(json.Value.GetRawText()) ?? new List<Electrode>() : new List<Electrode>();
        foreach (var e in Electrode)
        {
            Brush statusBrush = StatusColor(e.state);
            // 使用 SelectItem 的建構子（SelectItem 擁有 read-only 屬性與 constructor）
            items.Add(new SelectItem(e.electrodeName ?? string.Empty, e.state ?? string.Empty, statusBrush));
        }

        // 2) 建立視窗與 VM — 傳入 loader（否則視窗不會有資料）
        var dlg = new SelectItemWindow(SelectItemType.Electrode, _ => items)
        {
            Owner = _window
        };

        // 3) 顯示並取回選擇結果（加上 null 檢查避免 NRE）
        if (dlg.ShowDialog() == true)
        {
            SelectedElectrodeItem = dlg.Tag as SelectItem;

            var selectedName = SelectedElectrodeItem?.MaterialName ?? string.Empty;
            var e = Electrode.FirstOrDefault(x => string.Equals(x.electrodeName, selectedName, StringComparison.Ordinal));

            Lifetime = e?.lifeTimes?.ToString() ?? string.Empty;
            SelectStatus = e?.state ?? string.Empty;
            TagSerial = e?.tagSerial ?? string.Empty;
            Name = e?.electrodeName ?? string.Empty;
            PairEdm = e?.pairedEDM ?? string.Empty;
            Pgm = e?.edmpgm ?? string.Empty;
            // SelectedWorkpieceItem = null;
        }
    }
    [RelayCommand]
    private async Task SelectWorkOrder()
    {
        // 1) 準備資料
        var items = new List<WorksheetItem>();

        // 從 API 取得資料
        JsonElement? json = await _httpService.GetJsonAsync<JsonElement>("Worksheet/DB_GetAllWorkSheet", default);

        var worksheets = json.HasValue
            ? JsonSerializer.Deserialize<List<Worksheets>>(json.Value.GetRawText()) ?? new List<Worksheets>()
            : new List<Worksheets>();

        // 將 API model 轉成 UI 用的 WorksheetItem
        foreach (var ws in worksheets)
        {
            items.Add(new WorksheetItem
            {
                PartName = ws.workpieceName ?? string.Empty,
                WorkOrderNo = ws.worksheetNumber ?? string.Empty
            });
        }

        if (items.Count != 0)
        {
            // 2) 建立視窗與 VM
            var vm = new SelectWorksheetWindowViewModel(items);
            var dlg = new SelectWorksheetWindow
            {
                Owner = _window,
                DataContext = vm
            };

            // 3) 顯示並取回選擇結果
            if (dlg.ShowDialog() == true)
            {
                SelectedWorksheetItem = dlg.Tag as WorksheetItem;
            }
        }
    }

}
