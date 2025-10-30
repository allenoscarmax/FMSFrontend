using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Standard;
using FMSFrontend.Extensions; // SelectItemWindow / SelectItemType / SelectItem
using FMSFrontend.Interfaces;
using FMSFrontend.Services;
using FMSFrontend.ViewModels.Windows;
using FMSFrontend.Views;
using MaterialDesignThemes.Wpf.Transitions;
using OSCARMAXFMS_V3.DBmodels;
using OSCARMAXFMS_V3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
// 新增的 using
using System.Windows.Threading;

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
        // 每 0.5 秒輪詢 RFID 最新 Tag 並更新 TagSerial / 連線狀態
        _pollTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _pollTimer.Tick += PollTimer_Tick;
        _pollTimer.Start();
        // 當 Tag 有更新時，Tag 指示燈亮 3 秒後熄滅
        _tagOffTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        _tagOffTimer.Tick += TagOffTimer_Tick;
        if (_window != null) window.Closed += Window_Closed;
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
    [ObservableProperty] private string _selectName = string.Empty;
    [ObservableProperty] private string _selectPgm = string.Empty;
    [ObservableProperty] private string _selectPairEdm = string.Empty;
    // RFID 顯示屬性與計時器欄位
    [ObservableProperty] private Brush _rfidConnectedBrush = Brushes.Gray;
    [ObservableProperty] private Brush _rfidTagBrush = Brushes.Gray;
    private readonly DispatcherTimer _pollTimer;
    private readonly DispatcherTimer _tagOffTimer;
    Electrode electrode = new Electrode();
    Workpiece workpiece = new Workpiece();
    string SelecteId = "";
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
    private async Task Pair()
    {
        try
        {
            bool ok = false;
            if (ShowElectrodeSection)
            {
                // 電極配對頁面：更新 / 上傳電極資料
                if (SelectedElectrodeItem == null)
                {
                    _windowService.ShowMessage("請選擇要配對的電極");
                    return;
                }
                else if (SelectedWorksheetItem == null)
                {
                    _windowService.ShowMessage("請選擇要配對的工單");
                    return;
                }
                electrode.state = SelectStatus;
                electrode.worksheetNumber = SelectedWorksheetItem?.WorkOrderNo ?? string.Empty;
                electrode.tagSerial = TagSerial ?? "";
                ok = await _httpService.SendPutAsync("Electrode/DB_UpdateElectrodeData", electrode);
                if (!ok)
                {
                    _windowService.ShowMessage("電極上傳失敗: " + electrode.electrodeName);
                    return;
                }

            }
            else // Workpiece section
            {
                // 工件配對頁面：更新 / 上傳工件資料
                if (SelectedWorkpieceItem == null)
                {
                    _windowService.ShowMessage("請先選擇要配對的工件。");
                    return;
                }
                else if (SelectedWorksheetItem == null)
                {
                    _windowService.ShowMessage("請選擇要配對的工單");
                    return;
                }
                workpiece.status = SelectStatus;
                workpiece.worksheetNumber = SelectedWorksheetItem?.WorkOrderNo ?? string.Empty;
                workpiece.tagSerial = TagSerial ?? "";
                ok = await _httpService.SendPutAsync("Workpiece/DB_UpdateWorkpieceData", workpiece);
                if (!ok)
                {
                    _windowService.ShowMessage("工件上傳失敗: " + workpiece.workpieceName);
                    return;
                }
            }

            var RFIDWriteLog = new RFIDWriteLog
            {
                timeStamp = DateTime.Now,
                type = ShowElectrodeSection ? "Electrode" : "Workpiece",
                tagSerial = TagSerial ?? "",
                srialNo =  SelectedWorksheetItem?.WorkOrderNo ?? string.Empty,
                objName = SelectName ,
            };

            ok = await _httpService.SendPutAsync("RFIDMgmtModule/DB_InsertNewRFIDWriteLogData", RFIDWriteLog);
            if (!ok)
            {
                _windowService.ShowMessage("RFIDWriteLog上傳失敗: ");
                return;
            }
            _windowService.ShowMessage($"{CurrentTitle} OK!");
        }
        catch (Exception ex)
        {
            _windowService.ShowMessage("配對處理發生錯誤: " + ex.Message);
        }
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
    Brush StatusColor(string? s)
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
        var Workpiece = json.HasValue && json.Value.ValueKind != JsonValueKind.Undefined ? 
            JsonSerializer.Deserialize<List<Workpiece>>(json.Value.GetRawText()) ?? new List<Workpiece>() : 
            new List<Workpiece>();
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
            SelectName = wp?.workpieceName ?? string.Empty;
            SelectPairEdm = wp?.pairedEDM ?? string.Empty;
            SelectPgm = wp?.edmpgm ?? string.Empty;
            workpiece = wp!;
        }
    }

    [RelayCommand]
    private async Task SelectElectrode()
    {
        // 1) 準備資料
        var items = new List<SelectItem>();
        JsonElement? json = await _httpService.GetJsonAsync<JsonElement>("Electrode/DB_GetAllElectrode", default);
        var Electrode = json.HasValue && json.Value.ValueKind != JsonValueKind.Undefined ?
            JsonSerializer.Deserialize<List<Electrode>>(json.Value.GetRawText()) ?? new List<Electrode>() : 
            new List<Electrode>();
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
            SelectName = e?.electrodeName ?? string.Empty;
            SelectPairEdm = e?.pairedEDM ?? string.Empty;
            SelectPgm = e?.edmpgm ?? string.Empty;
            electrode = e!;
        }
    }
    [RelayCommand]
    private async Task SelectWorkOrder()
    {
        // 1) 準備資料
        var items = new List<WorksheetItem>();

        var worksheets = await _httpService.GetJsonAsync<List<Worksheets>>("Worksheet/DB_GetAllWorkSheet", default);
        foreach (var ws in worksheets)
        {
            items.Add(new WorksheetItem
            {
                PartName = ws.WorkpieceName ?? string.Empty,
                WorkOrderNo = ws.WorksheetNumber ?? string.Empty
            });
        }

        //從 API 取得資料
        //JsonElement? json = await _httpService.GetJsonAsync<JsonElement>("Worksheet/DB_GetAllWorkSheet", default);
        // if (!json.HasValue || json.Value.ValueKind == JsonValueKind.Undefined)
        // {
        //     _windowService.ShowMessage("請先新增工單");
        //     return;
        // }
        // else
        // {
        //     var worksheets = JsonSerializer.Deserialize<List<Worksheets>>(json.Value.GetRawText()) ?? new List<Worksheets>();
        //     // 將 API model 轉成 UI 用的 WorksheetItem
        //     foreach (var ws in worksheets)
        //     {
        //         items.Add(new WorksheetItem
        //         {
        //             PartName = ws.WorkpieceName ?? string.Empty,
        //             WorkOrderNo = ws.WorksheetNumber ?? string.Empty
        //         });
        //     }
        // }



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
    // 新增：計時器事件讀取RFID最新Tag
    private bool _isPolling = false;
    private async void PollTimer_Tick(object? sender, EventArgs e)
    {
        if (_isPolling) return; // 避重入
        _isPolling = true;
        try
        {
            string? json = null;
            try
            {
                RfidTagBrush = Brushes.Gray;
                // 取得原始回傳字串（API 回傳 body 為純文字 TagSerial）
                json = await _httpService.GetJsonAsyncNoDeserialize("RFIDMgmtModule/Read_Tag_ID/0/2", default);
            }
            catch
            {
                // 忽略單次錯誤（可加日誌）
                json = null;
            }
            if (!string.IsNullOrWhiteSpace(json))
            {
                TagSerial = json;
                RfidTagBrush = Brushes.LimeGreen;
            }
            else
            {
                // 若沒有讀到 tag，顯示未連線/灰色（但不要覆蓋現有 TagSerial）
                RfidTagBrush = Brushes.Gray;
            }
            JsonElement? json2;
            try
            {
                json2 = await _httpService.GetJsonAsync<JsonElement>("RFIDMgmtModule/GetRFIDParas", default);
                RFIDParas r = (json2.HasValue && json2.Value.ValueKind != JsonValueKind.Undefined) ?
                      JsonSerializer.Deserialize<RFIDParas>(json2.Value.GetRawText()) ?? new RFIDParas() :
                      new RFIDParas();
                 RfidConnectedBrush = r.RFID_Is_connect[2] ? Brushes.LimeGreen : RfidConnectedBrush = Brushes.Gray;
            }
            catch
            {
                // 忽略單次錯誤（可加日誌）
                json2 = null;
            }
        }
        finally
        {
            _isPolling = false;
        }
    }   
    
    private void TagOffTimer_Tick(object? sender, EventArgs e)
    {
        _tagOffTimer.Stop();
        RfidTagBrush = Brushes.Gray;
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        _pollTimer?.Stop();
        _tagOffTimer?.Stop();
        if (_window != null)
        {
            _window.Closed -= Window_Closed;
        }
    }
}
