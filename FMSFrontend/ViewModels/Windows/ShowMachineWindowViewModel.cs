using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
// 👇 依你的實際命名空間調整
using FMSFrontend.ViewModels;
using FMSFrontend.ViewModels.Production;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace FMSFrontend.ViewModels.Windows
{
    public partial class ShowMachineWindowViewModel : ObservableObject
    {
        [ObservableProperty] private string deviceName = "設備名稱";
        [ObservableProperty] private MachineInfo info = new();
        [ObservableProperty] private bool showAllItems = true;
        [ObservableProperty] private bool showByDate;
        [ObservableProperty] private DateTime? selectedDate = DateTime.Today;

        public ObservableCollection<WorkOrderRow> WorkOrders { get; } = new();

        public ShowMachineWindowViewModel() { }

        // ✅ 這裡把 MachineCard 的資料接進來
        public ShowMachineWindowViewModel(object? machineCard)
        {
            if (machineCard is MachineCardViewModel m)
            {
                DeviceName = m.MachineName ?? "設備名稱";

                Info.MachineTypeName = m.MachineTypeName;       // 給 Converter 用（EDM/CNC/ZNC…）
                Info.EquipmentName = m.MachineName;           // 左卡「設備名稱」
                Info.EquipmentType = m.Type.ToString();       // 左卡「設備類型」
                Info.MachineState = MapStatus(m.Status);     // 左卡「機台狀態」

                // 其它目前 MachineCard 沒提供的欄位先給預設 / 空值
                Info.ModelNo = string.Empty;
                Info.CurrentProgram = string.Empty;
                Info.StateDuration = string.Empty;
                Info.CurrentElectrode = string.Empty;
                Info.CurrentWork = string.Empty;
                Info.WaitingCount = 0;
                Info.FinishedToday = 0;
                Info.AssetNo = string.Empty;
            }
            else
            {
                // 若沒帶入卡片 VM，保留預設
                DeviceName = "設備名稱";
            }
        }

        private static string MapStatus(string? s) => (s ?? "").ToLowerInvariant() switch
        {
            "idle" => "待機",
            "running" => "運轉中",
            "warning" => "警告",
            "error" => "異常",
            "disabled" => "禁用",
            _ => s ?? ""
        };

        [RelayCommand] private void ForceElectrodeOut() { /* TODO */ }
        [RelayCommand] private void ForceWorkOut() { /* TODO */ }
        [RelayCommand] private void CloseWindow(Window? w) => w?.Close();
    }

    public partial class MachineInfo : ObservableObject
    {
        [ObservableProperty] private string machineTypeName = "EDM";
        [ObservableProperty] private string equipmentName = "";
        [ObservableProperty] private string equipmentType = "";
        [ObservableProperty] private string modelNo = "";
        [ObservableProperty] private string currentProgram = "";
        [ObservableProperty] private string machineState = "";
        [ObservableProperty] private string stateDuration = "";
        [ObservableProperty] private string currentElectrode = "";
        [ObservableProperty] private string currentWork = "";
        [ObservableProperty] private int waitingCount;
        [ObservableProperty] private int finishedToday;
        [ObservableProperty] private string assetNo = "";
    }

    public class WorkOrderRow
    {
        public string CreatedAtText { get; set; } = "";
        public string WorkOrderNo { get; set; } = "";
        public string Status { get; set; } = "";
        public string JobNo { get; set; } = "";
    }
}
