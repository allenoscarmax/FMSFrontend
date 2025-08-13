using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using FMSFrontend.Extensions; // SelectItemWindow / SelectItemType / SelectItem
using FMSFrontend.Interfaces;
using FMSFrontend.ViewModels.Windows;

public partial class MaterialPairViewModel : ObservableObject
{
    private readonly Window _window;
    private readonly IWindowService _windowService;

    public MaterialPairViewModel(bool isElectrode, Window window, IWindowService windowService)
    {
        ShowElectrodeSection = isElectrode;
        ShowWorkpieceSection = !isElectrode;
        _window = window;
        _windowService = windowService;
    }

    // 控制區塊顯示
    [ObservableProperty] private bool showElectrodeSection;
    [ObservableProperty] private bool showWorkpieceSection;

    // 使用者選到的項目（給後續 Pair 或 UI 顯示）
    [ObservableProperty] private SelectItem? selectedElectrodeItem;
    [ObservableProperty] private SelectItem? selectedWorkpieceItem;
    // ★ 將 SelectItem 換成 WorksheetItem
    [ObservableProperty] private WorksheetItem? selectedWorksheetItem;


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

    [RelayCommand]
    private void SelectWorkpiece()
    {
        // 若你有資料來源，可傳一個 loader 進去；沒有就直接 new
        // 例：var dlg = new SelectItemWindow(SelectItemType.Workpiece, LoadItems);
        var dlg = new SelectItemWindow(SelectItemType.Workpiece)
        {
            Owner = _window
        };

        if (dlg.ShowDialog() == true)
        {
            SelectedWorkpieceItem = dlg.Tag as SelectItem;
            // 若切換選擇邏輯需要清掉另一邊，可視需求做：
            // SelectedElectrodeItem = null;
        }
    }

    [RelayCommand]
    private void SelectElectrode()
    {
        var dlg = new SelectItemWindow(SelectItemType.Electrode)
        {
            Owner = _window
        };

        if (dlg.ShowDialog() == true)
        {
            SelectedElectrodeItem = dlg.Tag as SelectItem;
            // SelectedWorkpieceItem = null;
        }
    }
    [RelayCommand]
    private void SelectWorkOrder()
    {
        // 1) 準備資料（此處放假資料；換成你的服務/Repository 即可）
        IEnumerable<WorksheetItem> items = null;

        // 2) 建立視窗與 VM
        var vm = new SelectWorksheetWindowViewModel(items);
        var dlg = new SelectWorksheetWindow
        {
            Owner = _window,                 // 若沒有 _window，可用 Application.Current.MainWindow
            DataContext = vm
        };

        // 3) 顯示並取回選擇結果
        if (dlg.ShowDialog() == true)
        {
            SelectedWorksheetItem = dlg.Tag as WorksheetItem;
            // 若需要互斥清空其他選擇，可在這裡處理
        }
    }

}
