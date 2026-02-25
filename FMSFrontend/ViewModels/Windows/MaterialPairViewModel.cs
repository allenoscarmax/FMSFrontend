using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Standard;
using FMSFrontend.Extensions; // SelectItemWindow / SelectItemType / SelectItem
using FMSFrontend.Interfaces;
using FMSFrontend.Models;
using FMSFrontend.Services;
using FMSFrontend.ViewModels.Windows;
using FMSFrontend.Views;
using FMSFrontend.Views.Windows;
using MaterialDesignThemes.Wpf.Transitions;
//using OSCARMAXFMS_V3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using FMSFrontend.Features.Services;
using FMSFrontend.Helpers;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;
using FMSFrontend.Features.Dtos;

// 新增的 using
using System.Windows.Threading;

public partial class MaterialPairViewModel : ObservableObject
{
    private readonly IWindowService _windowService;
    private readonly IHttpService _httpService;
    // === Services ===
    private readonly IElectrodeService _electrodeService;
    private readonly IProbeService _probeService;
    private readonly IRfidService _rfidService;
    private readonly IWorkpieceService _workpieceService;
    private readonly IWorksheetsService _worksheetService;
    private readonly IPlcService _plcService;

    // === Singleton ===
    public RFIDBindStore RfidBindStore { get; } = new();
    public RFIDBindModel RfidBindmodel => RfidBindStore.RfidBind;
    // ==LiveUpdater===
    public RFIDBindLiveUpdater _rfidUpdater;

    public Action? CloseAction { get; set; }
    public MaterialPairViewModel(IWindowService windowService, IHttpService httpService,
        IElectrodeService electrodeService, IProbeService probeService, IRfidService rfidService,
        IWorkpieceService workpieceService, IWorksheetsService worksheetService, IPlcService plcService,
        RFIDBindStore rFIDBindStore, RFIDBindLiveUpdater rFIDBindLiveUpdater )
    {
        _windowService = windowService;
        _httpService = httpService;
        // === Services ===
        _electrodeService = electrodeService;
        _probeService = probeService;
        _rfidService = rfidService;
        _workpieceService = workpieceService;
        _worksheetService = worksheetService;
        _plcService = plcService;
        // === Singleton ===
        RfidBindStore = rFIDBindStore;
        // ==LiveUpdater===
        _rfidUpdater = rFIDBindLiveUpdater;
    }
    public void Initialize(bool isElectrode)
    {
        ShowElectrodeSection = isElectrode;
        ShowWorkpieceSection = !isElectrode;

        // 依模式設定 Updater 狀態
        _rfidUpdater.IsElectrode = isElectrode;

        // 如果需要初始化資料，可以在這裡做：
        // LoadElectrodeListAsync() / LoadWorkpieceListAsync() 等
    }

    public void OnPageActivated()
    {
        _rfidUpdater.Start();
        _rfidUpdater.ReadTagFlag = true;

    }
    public void OnPageDeactivated()
    {
        _rfidUpdater.Stop();
        _rfidUpdater.ReadTagFlag = false;
    }

    // 控制區塊顯示
    [ObservableProperty] private bool showElectrodeSection;
    [ObservableProperty] private bool showWorkpieceSection;
    [ObservableProperty] private bool selectEnable = false;


    // 使用者選到的項目（給後續 Pair 或 UI 顯示）
    [ObservableProperty] private SelectItem? selectedElectrodeItem;
    [ObservableProperty] private SelectItem? selectedWorkpieceItem;
    // ★ 將 SelectItem 換成 WorksheetItem
    [ObservableProperty] private WorksheetItem? selectedWorksheetItem;

    [ObservableProperty] private string _lifetime = string.Empty;
    [ObservableProperty] private string _selectStatus = string.Empty;
    [ObservableProperty] private string _selectName = string.Empty;
    [ObservableProperty] private string _selectPgm = string.Empty;
    [ObservableProperty] private string _selectPairEdm = string.Empty;
    // RFID 顯示屬性與計時器欄位
    //[ObservableProperty] private string _tagSerial = string.Empty; // 改用 RFIDBindmodel.TagSerial
    //[ObservableProperty] private Brush _rfidTagBrush = Brushes.Gray;// 改用 RFIDBindmodel.TagBrush
    //[ObservableProperty] private Brush _rfidConnectedBrush = Brushes.Gray;// 改用 RFIDBindmodel.ConnectedBrush
    ElectrodeDto electrode = new ElectrodeDto();
    WorkpieceDto workpiece = new WorkpieceDto();
    // 方便 UI 綁定顯示文字（可選）
    public string CurrentTitle => ShowElectrodeSection
        ? LanguageManager.GetString("MaterialPair_Title_Electrode", "電極配對")
        : LanguageManager.GetString("MaterialPair_Title_Workpiece", "工件配對");


    // 顯示用屬性（還沒選時有預設文字）
    public string SelectedElectrodeNameDisplay =>
        SelectedElectrodeItem?.MaterialName
        ?? LanguageManager.GetString("MaterialPair_Message_SelectElectrode", "請選擇電極");

    public string SelectedWorkpieceNameDisplay =>
        SelectedWorkpieceItem?.MaterialName
        ?? LanguageManager.GetString("MaterialPair_Message_SelectWorkpiece", "請選擇工件");
    // ★ 改用 WorksheetItem 欄位
    public string SelectedWorkOrderName =>
        SelectedWorksheetItem != null
            ? string.Format(
                CultureInfo.CurrentCulture,
                LanguageManager.GetString("MaterialPair_SelectedWorkOrder_Format", "{0}（{1}）"),
                SelectedWorksheetItem.PartName,
                SelectedWorksheetItem.WorkOrderNo)
            : LanguageManager.GetString("MaterialPair_Message_SelectWorkOrder", "請選擇工單");

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
        SelectEnable = SelectedWorksheetItem != null;
    }
    // Pair
    [RelayCommand]
    private async Task BalluffReset()
    {
        try
        {
            bool ok = await _plcService.BalluffPowerAsync(true);
            await Task.Delay(1000);
            ok = await _plcService.BalluffPowerAsync(false);
            await Task.Delay(1000);
            ok = await _rfidService.RFID_to_disconnect(0);
            await Task.Delay(300);
            ok = await _rfidService.RFID_to_connect(0);

            _windowService.ShowMessage(LanguageManager.GetString("MaterialPair_Message_ResetOk", "OK"));
        }
        catch { }
    }

    // Pair
    [RelayCommand]
    private async Task Pair()
    {
        try
        {
            bool ok = false;
            if (RfidBindmodel.TagSerial == null || RfidBindmodel.TagSerial == "")
            {
                _windowService.ShowMessage(LanguageManager.GetString("MaterialPair_Message_NoTagSerial", "無標籤序號"));
                return;
            }
            if (ShowElectrodeSection)
            {
                // 電極配對頁面：更新 / 上傳電極資料
                if (SelectedElectrodeItem == null)
                {
                    _windowService.ShowMessage(LanguageManager.GetString("MaterialPair_Message_SelectPairElectrode", "請選擇要配對的電極"));
                    return;
                }
                else if (SelectedWorksheetItem == null)
                {
                    _windowService.ShowMessage(LanguageManager.GetString("MaterialPair_Message_SelectPairWorkOrder", "請選擇要配對的工單"));
                    return;
                }

                // 解除既有電極的 TagSerial 綁定
                var existingElectrodes = await _electrodeService.DB_GetElectrodesByTagSerialAsync(RfidBindmodel.TagSerial ?? "");

                while (existingElectrodes != null)
                {
                    if (existingElectrodes != null && existingElectrodes.Count > 0)
                    {
                        foreach (var dto in existingElectrodes)
                        {
                            dto.state = "OffShelf";
                            dto.tagSerial = "";
                            try
                            {
                                ok = await _electrodeService.DB_UpdateElectrodeDataAsync(dto);
                                if (!ok)
                                {
                                    _windowService.ShowMessage(LanguageManager.GetString("MaterialPair_Message_ElectrodeUploadFailed", "電極上傳失敗"));
                                    return;
                                }
                            }
                            catch { }
                        }
                    }
                    existingElectrodes = await _electrodeService.DB_GetElectrodesByTagSerialAsync(RfidBindmodel.TagSerial ?? "");
                }

                // 更新目前選到的電極資料
                electrode.state = "Verified";
                electrode.worksheetNumber = SelectedWorksheetItem?.WorkOrderNo ?? string.Empty;
                electrode.tagSerial = RfidBindmodel.TagSerial ?? "";
                electrode.currentLocation = "OffShelf";
                try
                {
                    ok = await _electrodeService.DB_UpdateElectrodeDataAsync(electrode);
                    if (!ok)
                    {
                        _windowService.ShowMessage(LanguageManager.GetString("MaterialPair_Message_ElectrodeUploadFailed", "電極上傳失敗"));
                        return;
                    }
                }
                catch { }
            }
            else // Workpiece section
            {
                // 工件配對頁面：更新 / 上傳工件資料
                if (SelectedWorkpieceItem == null)
                {
                    _windowService.ShowMessage(LanguageManager.GetString("MaterialPair_Message_SelectPairWorkpiece", "請先選擇要配對的工件。"));
                    return;
                }
                else if (SelectedWorksheetItem == null)
                {
                    _windowService.ShowMessage(LanguageManager.GetString("MaterialPair_Message_SelectPairWorkOrder", "請選擇要配對的工單"));
                    return;
                }
                    // 解除既有工件的 TagSerial 綁定
                    var existingWp = await _workpieceService.GetWorkpieceByTagSerialAsync(RfidBindmodel.TagSerial ?? "");

                while (existingWp != null)
                {

                    existingWp.status = "OffShelf";
                    existingWp.tagSerial = "";
                    try
                    {
                        ok = await _workpieceService.UpdateWorkpieceDataAsync(existingWp);
                        if (!ok)
                        {
                            _windowService.ShowMessage(LanguageManager.GetString("MaterialPair_Message_ElectrodeUploadFailed", "電極上傳失敗"));
                            return;
                        }
                    }
                    catch { }
                    existingWp = await _workpieceService.GetWorkpieceByTagSerialAsync(RfidBindmodel.TagSerial ?? "");
                }
                // 更新目前選到的工件資料
                workpiece.status = "Verified";
                workpiece.worksheetNumber = SelectedWorksheetItem?.WorkOrderNo ?? string.Empty;
                workpiece.tagSerial = RfidBindmodel.TagSerial ?? "";
                workpiece.currentLocation = "onASE1";

                ok = await _workpieceService.UpdateWorkpieceDataAsync(workpiece);
                if (!ok)
                {
                    _windowService.ShowMessage(LanguageManager.GetString("MaterialPair_Message_WorkpieceUploadFailed", "工件上傳失敗"));
                    return;
                }

                //更新工單狀態
                var existingWorksheet = await _worksheetService.GetWorkSheetByWorkSheetNumberAsync(SelectedWorksheetItem?.WorkOrderNo ?? string.Empty);
                if (existingWorksheet != null)
                {
                    existingWorksheet.workStatus = "Queue";
                    try
                    {
                        ok = await _worksheetService.UpdateWorkSheetDataAsync(existingWorksheet);
                        if (!ok)
                        {
                            _windowService.ShowMessage(LanguageManager.GetString("MaterialPair_Message_ElectrodeUploadFailed", "電極上傳失敗"));
                            return;
                        }
                    }
                    catch { }
                }
            }
            // 上傳 RFIDWriteLog
            var RFIDWriteLog = new RFIDWriteLogDto
            {
                timeStamp = DateTime.Now,
                type = ShowElectrodeSection ? "Electrode" : "Workpiece",
                tagSerial = RfidBindmodel.TagSerial ?? "",
                srialNo =  SelectedWorksheetItem?.WorkOrderNo ?? string.Empty,
                objName = SelectName ,
            };

            ok = await _rfidService.InsertNewRFIDWriteLogDataAsync(RFIDWriteLog);
            if (!ok)
            {
                _windowService.ShowMessage(LanguageManager.GetString("MaterialPair_Message_RfidWriteLogFailed", "RFIDWriteLog上傳失敗: "));
                return;
            }
            var successMessage = string.Format(
                CultureInfo.CurrentCulture,
                LanguageManager.GetString("MaterialPair_Message_PairSuccess", "{0} OK!"),
                CurrentTitle);
            _windowService.ShowMessage(successMessage);
        }
        catch (Exception ex)
        {
            var message = string.Format(
                CultureInfo.CurrentCulture,
                LanguageManager.GetString("MaterialPair_Message_PairError", "配對處理發生錯誤: {0}"),
                ex.Message);
            _windowService.ShowMessage(message);
        }
    }

    [RelayCommand]
    private void Back()
    {
        // 用 CloseAction 關閉視窗（MVVM 正規方法）
        CloseAction?.Invoke();

        // 先開「選擇物料型態」視窗
        if (!_windowService.ShowMaterialTypeSelectWindow(out MaterialKind kind))
            return;

        // 根據使用者選擇，再開對應的 Pair 視窗（全部交給 WindowService）
        switch (kind)
        {
            case MaterialKind.Electrode:
                _windowService.ShowMaterialPairWindow(isElectrode: true);
                break;

            case MaterialKind.Workpiece:
                _windowService.ShowMaterialPairWindow(isElectrode: false);
                break;

            case MaterialKind.Probe:
                _windowService.ShowProbePairWindow();
                break;

            default:
                return;
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
            "Booked" => Brushes.Gray,
            "Empty" => Brushes.White,
            _ => Brushes.White
        };
    }

    [RelayCommand]
    private async Task SelectWorkpiece()
    {
        try
        {
            if (SelectedWorksheetItem == null)
            {
                _windowService.ShowMessage(LanguageManager.GetString("MaterialPair_Message_SelectWorkOrder", "請選擇工單"));
                return;
            }

            // 1) 準備資料（此 API 回傳單筆 WorkpieceDto）
            var items = new List<SelectItem>();
            WorkpieceDto? wp = await _workpieceService
                .GetWorkpieceByWorksheetNumberAsync(SelectedWorksheetItem.WorkOrderNo);

            if (wp != null)
            {
                Brush statusBrush = StatusColor(wp.status);
                items.Add(new SelectItem(
                    wp.workpieceName ?? string.Empty,
                    wp.status ?? string.Empty,
                    statusBrush));
            }
            else
            {
                _windowService.ShowMessage(LanguageManager.GetString("MaterialPair_Message_ApiNoData", "API 無資料"));
                return;
            }

            // 2) 交給 WindowService 顯示選擇視窗
            var selected = _windowService.ShowSelectItemWindow(SelectItemType.Workpiece, items);
            if (selected == null)
                return;

            SelectedWorkpieceItem = selected;

            var selectedName = SelectedWorkpieceItem.MaterialName ?? string.Empty;

            if (wp != null && string.Equals(wp.workpieceName, selectedName, StringComparison.Ordinal))
            {
                SelectStatus = wp.status ?? string.Empty;
                RfidBindmodel.TagSerial = wp.tagSerial ?? string.Empty;
                SelectName = wp.workpieceName ?? string.Empty;
                SelectPairEdm = wp.pairedEDM ?? string.Empty;
                SelectPgm = wp.edmpgm ?? string.Empty;
                workpiece = wp;
            }
            else
            {
                _windowService.ShowMessage(LanguageManager.GetString("MaterialPair_Message_WorkpieceNotFound", "找不到工件"));
            }
        }
        catch
        {
            _windowService.ShowMessage(LanguageManager.GetString("MaterialPair_Message_SelectWorkpieceFailed", "工件選擇失敗"));
        }
    }


    [RelayCommand]
    private async Task SelectElectrode()
    {
        if (SelectedWorksheetItem == null)
        {
            _windowService.ShowMessage(LanguageManager.GetString("MaterialPair_Message_SelectWorkOrder", "請選擇工單"));
            return;
        }

        try
        {
            var items = new List<SelectItem>();

            // 1) 準備資料
            List<ElectrodeDto> electrodes =
                await _electrodeService.GetElectrodeByWorksheetNumberAsync(SelectedWorksheetItem.WorkOrderNo)
                ?? new List<ElectrodeDto>();

            foreach (var e in electrodes)
            {
                if (!string.IsNullOrEmpty(e.electrodeName) && e.electrodeName.Contains("-01") && e.shared == true)
                    continue;

                Brush statusBrush = StatusColor(e.state);
                items.Add(new SelectItem(
                    e.electrodeName ?? string.Empty,
                    e.state ?? string.Empty,
                    statusBrush));
            }

            if (items.Count == 0)
            {
                _windowService.ShowMessage(LanguageManager.GetString("MaterialPair_Message_NoElectrodeForWorkOrder", "此工單沒有可配對的電極"));
                return;
            }

            // 2) 交給 WindowService 顯示選擇視窗
            var selected = _windowService.ShowSelectItemWindow(SelectItemType.Electrode, items);
            if (selected == null)
                return;

            SelectedElectrodeItem = selected;

            var selectedName = SelectedElectrodeItem.MaterialName ?? string.Empty;
            var eDto = electrodes.FirstOrDefault(
                x => string.Equals(x.electrodeName, selectedName, StringComparison.Ordinal));

            Lifetime = eDto?.lifeTimes?.ToString() ?? string.Empty;
            SelectStatus = eDto?.state ?? string.Empty;
            RfidBindmodel.TagSerial = eDto?.tagSerial ?? string.Empty;
            SelectName = eDto?.electrodeName ?? string.Empty;
            SelectPairEdm = eDto?.pairedEDM ?? string.Empty;
            SelectPgm = eDto?.edmpgm ?? string.Empty;
            electrode = eDto!;
        }
        catch
        {
            _windowService.ShowMessage(LanguageManager.GetString("MaterialPair_Message_SelectElectrodeFailed", "選擇電極失敗"));
        }
    }
    // 取得尾碼：預設回傳 "02"（不帶連字號）；若你想帶 "-02" 改 return parts[^1] 前面加 "-"
    private static string ExtractTailSuffix(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return string.Empty;

        // 以最後一個 '-' 切
        int idx = fullName.LastIndexOf('-');
        if (idx < 0 || idx == fullName.Length - 1) return fullName;

        var part = fullName.Substring(idx + 1);
        // 僅保留兩碼數字（若有需要）
        // if (part.Length >= 2) part = part[^2..];
        return part; // 回 "02"
    }
    [RelayCommand]
    private async Task SelectWorkOrder()
    {
        try
        {
            // 1) 準備資料
            var items = new List<WorksheetItem>();
            List<WorksheetsDto> worksheets = await _worksheetService.GetAllWorkSheetAsync() ?? new List<WorksheetsDto>();
            foreach (var ws in worksheets)
            {
                if (ws.workStatus != "Completed" && ws.workStatus != "Failure")
                {
                    items.Add(new WorksheetItem
                    {
                        PartName = ws.workpieceName ?? string.Empty,
                        WorkOrderNo = ws.worksheetNumber ?? string.Empty
                    });
                }
            }
            if (items.Count == 0)
            {
                _windowService.ShowMessage(LanguageManager.GetString("MaterialPair_Message_NoAvailableWorkOrders", "目前沒有可選擇的工單"));
                return;
            }

            // 2) 交給 WindowService 顯示選擇視窗，回傳使用者選擇
            var selected = _windowService.ShowSelectWorksheetWindow(items);
            if (selected != null)
            {
                SelectedWorksheetItem = selected;
            }
        }
        catch 
        {
            _windowService.ShowMessage(LanguageManager.GetString("MaterialPair_Message_SelectWorkOrderFailed", "選擇工單失敗"));
        }
        SelectedElectrodeItem = null;
        SelectedWorkpieceItem = null;
    }
    /*
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
    */
}
