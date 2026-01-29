using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Extensions;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Interfaces;
using FMSFrontend.Models;          // WorksheetItem、SelectItem
using FMSFrontend.Services;       // IWorksheetService、IElectrodeService
using FMSFrontend.Views.Windows;  // SelectWorksheetWindow、SelectItemWindow
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace FMSFrontend.ViewModels.Windows
{
    public partial class SelectSharedElectrodeViewModel : ObservableObject
    {
        private readonly IWorksheetsService _worksheetService;
        private readonly IElectrodeService _electrodeService;
        private readonly IWindowService _windowService;
        public string _targetElectrodeName; // Share 時傳入的電極名稱

        // 🔹 顯示文字用屬性
        [ObservableProperty] private string? selectedWorkOrderName = "請選擇工單";
        [ObservableProperty] private string? selectedElectrodeNameDisplay = "請選擇電極";

        // 🔹 內部狀態
        public WorksheetItem? SelectedWorksheetItem { get; private set; }
        public SelectItem? SelectedElectrodeItem { get; private set; }

        private int ElectrodesFilter = 5; //佑義:2 鋐興:5
        // ------------------------------------------------------------
        // 📦 暫存資料區：在 ViewModel 開頭統一定義
        // ------------------------------------------------------------
        private List<WorksheetsDto> _cachedWorksheets = new();  // 工單暫存
        private List<ElectrodeDto> _cachedElectrodes = new();   // 電極暫存


        public event EventHandler<SharedElectrodeSelection>? CloseRequested;

        public SelectSharedElectrodeViewModel(
            IWindowService windowService,
            IWorksheetsService worksheetService,
            IElectrodeService electrodeService)
        {
            _windowService = windowService;
            _worksheetService = worksheetService;
            _electrodeService = electrodeService;
        }

        public void Initialize(string electrodeName)
        {
            _targetElectrodeName = electrodeName;
            // 如果這裡要做資料讀取，也能一併觸發
        }

        // ------------------------------------------------------------
        private static string ExtractWorkpieceNameFromElectrodeName(string? electrodeName)
        {
            if (string.IsNullOrWhiteSpace(electrodeName))
                return string.Empty;

            // 方式一：最快，找第一個底線
            int idx = electrodeName.IndexOf('_');
            if (idx > 0)
                return electrodeName.Substring(0, idx);

            // 方式二（防極端格式）：取到 "-01"、"-02" 前面的主體再去掉後段，但通常不會走到這
            // var m = Regex.Match(electrodeName, @"^(.+?)_");
            // return m.Success ? m.Groups[1].Value : electrodeName;

            return electrodeName;
        }
        [RelayCommand]
        private async Task SelectWorkOrderAsync()
        {
            try
            {
                // 從 electrodeName 萃取工件名稱（第一個 '_' 之前的字串）
                var workpieceName = ExtractWorkpieceNameFromElectrodeName(_targetElectrodeName);
                if (string.IsNullOrEmpty(workpieceName))
                    return;

                _cachedWorksheets = await _worksheetService
                    .DB_GetWorkSheetsbyContainWorkpieceName(workpieceName)
                    ?? new List<WorksheetsDto>();

                var items = _cachedWorksheets
                    .Select(ws => new WorksheetItem
                    {
                        PartName = ws.workpieceName ?? string.Empty,
                        WorkOrderNo = ws.worksheetNumber ?? string.Empty
                    })
                    .ToList();

                // 移除自己的工單編號（依照你原本邏輯）
                string[] sr = _targetElectrodeName.Split('-');
                if (sr.Length > 2)
                    items.RemoveAll(x => x.PartName.IndexOf(sr[0] + "-" + sr[1], StringComparison.Ordinal) == 0);

                if (items.Count == 0)
                    return;
                //檢查shared electrode是否已被使用
                for (int i = 0; i < items.Count; i++)
                {
                    var es = await _electrodeService.GetElectrodeByWorksheetNumberAsync(items[i].WorkOrderNo ?? string.Empty) ?? new List<ElectrodeDto>(); // 取得該工單的電極清單
                    foreach (var e in es)//如果電及已經被共用 移除item
                    {
                        if (e.shared)
                        {
                            items.RemoveAt(i);
                            i--;//因為移除一個item 所以index要往前移動一格
                            break;
                        }
                    }
                }

                // ✅ 改用 WindowService
                var selected = _windowService.ShowSelectWorksheetWindow(items);
                if (selected == null)
                    return;

                SelectedWorksheetItem = selected;
                SelectedWorkOrderName = SelectedWorksheetItem.WorkOrderNo ?? "請選擇工單";

                // 清空電極
                SelectedElectrodeItem = null;
                SelectedElectrodeNameDisplay = "請選擇電極";
            }
            catch
            {
                _windowService.ShowMessage("選擇工單失敗");
            }
        }


        // ------------------------------------------------------------
        // 幫忙把完整電極名 ↔ 尾碼做對應用
        private readonly Dictionary<string, string> _tailToFullName = new();
        [RelayCommand]
        private async Task SelectElectrodeAsync()
        {
            try
            {
                if (SelectedWorksheetItem == null)
                    return;

                _cachedElectrodes = await _electrodeService.DB_GetAllElectrodeAsync()
                                    ?? new List<ElectrodeDto>();

                var items = new List<SelectItem>();

                foreach (var e in _cachedElectrodes)
                {
                    // 同工單
                    if (!string.Equals(e.worksheetNumber, SelectedWorksheetItem.WorkOrderNo, StringComparison.Ordinal))
                        continue;

                    // 只挑尾碼為 -02
                    if (string.IsNullOrWhiteSpace(e.electrodeName) ||
                        !e.electrodeName.EndsWith("-" + ElectrodesFilter.ToString("D2"), StringComparison.OrdinalIgnoreCase))
                        continue;

                    // lifeTimes > useTimes
                    int life = e.lifeTimes ?? 0;
                    int used = e.useTimes ?? 0;
                    if (life <= used)
                        continue;

                    // ✅ 顯示電極全名
                    Brush brush = StatusColor(e.state);
                    items.Add(new SelectItem(e.electrodeName, e.state ?? string.Empty, brush));
                }

                if (items.Count == 0)
                    return;

                // ✅ 改用 WindowService
                var selected = _windowService.ShowSelectItemWindow(SelectItemType.Electrode, items);
                if (selected == null)
                    return;

                SelectedElectrodeItem = selected;
                SelectedElectrodeNameDisplay = SelectedElectrodeItem.MaterialName ?? "請選擇電極";
            }
            catch
            {
                _windowService.ShowMessage("選擇電極失敗");
            }
        }


        // ------------------------------------------------------------
        [RelayCommand]
        private void ConfirmSelect()
        {
            if (SelectedWorksheetItem == null || SelectedElectrodeItem == null)
                return;

            WorksheetsDto wsDto = _cachedWorksheets.FirstOrDefault(w =>
                string.Equals(w.worksheetNumber, SelectedWorksheetItem.WorkOrderNo, StringComparison.Ordinal)) ?? new();

            ElectrodeDto eleDto = _cachedElectrodes.FirstOrDefault(e =>
                string.Equals(e.electrodeName, SelectedElectrodeItem.MaterialName, StringComparison.Ordinal)) ?? new();


            var result = new SharedElectrodeSelection
            {
                WorkOrderId = wsDto._id,
                WorkOrderNo = wsDto.worksheetNumber,
                WorkOrderName = wsDto.workpieceName,
                ElectrodeId = eleDto._id,
                ElectrodeName = eleDto.electrodeName,
                ElectrodeState = eleDto.state
            };

            CloseRequested?.Invoke(this, result);
        }

        // ------------------------------------------------------------
        private static Brush StatusColor(string? state)
        {
            return state?.ToLowerInvariant() switch
            {
                "new" => new SolidColorBrush(Color.FromRgb(54, 179, 126)),     // 綠色
                "used" => new SolidColorBrush(Color.FromRgb(255, 165, 0)),     // 橘色
                "deprecated" => new SolidColorBrush(Color.FromRgb(220, 53, 69)),// 紅色
                _ => Brushes.Gray
            };
        }
    }

    // 回傳型別（可放在同檔）
    public class SharedElectrodeSelection
    {
        public string WorkOrderId { get; set; } = "";
        public string WorkOrderNo { get; set; } = "";
        public string WorkOrderName { get; set; } = "";
        public string ElectrodeId { get; set; } = "";
        public string ElectrodeName { get; set; } = "";
        public string ElectrodeState { get; set; } = "";
    }
}
