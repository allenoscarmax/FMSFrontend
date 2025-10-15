using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Controls;
using FMSFrontend.Interfaces;
using FMSFrontend.Models;
using FMSFrontend.ViewModels.Production;
using FMSFrontend.ViewModels.Windows;
using FMSFrontend.Views;
using FMSFrontend.Views.Windows;
using IniFile;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using static FMSFrontend.ViewModels.ElectrodeDetailViewModel;

namespace FMSFrontend.ViewModels
{
    public partial class ProductionLinesViewModel : ObservableObject
    {
        private object _currentStorageView;
        public object CurrentStorageView
        {
            get => _currentStorageView;
            set => SetProperty(ref _currentStorageView, value);
        }
        private object _currentWorkingZoneView;
        public object CurrentWorkingZoneView
        {
            get => _currentWorkingZoneView;
            set => SetProperty(ref _currentWorkingZoneView, value);
        }

        public readonly IWindowService _windowService;
        public ProductionLinesViewModel(IWindowService windowService)
        {
            _windowService = windowService;

            // 預設載入總覽畫面
            INIFile ini = new INIFile(AppDomain.CurrentDomain.BaseDirectory + "\\Basesitting.ini");
            bool b = ini.Read("Prarm", "IsStorageOverviewControl") == "True";
            if (b) 
                ShowOverview(); 
            else 
                ShowDetail("0");
            ShowMachineOverview();
        }


        public class RobotStatusViewModel
        {
            public MachineStatus Status { get; set; }
        }
        public enum MachineStatus
        {
            Idle,
            Running,
            Alarm
        }

        // 你原本給 Overview 用的：
        public void OpenMaterial(StorageSlotViewModel slot) => OpenMaterial((IHasMaterial)slot);

        // ★ Detail 也能用：
        public void OpenMaterial(SlotViewModel slot) => OpenMaterial((IHasMaterial)slot);

        // ★ 統一處理（Empty 也能開）
        public void OpenMaterial(IHasMaterial slot)
        {
            var material = slot?.Material;
            if (material == null)
            {
                _windowService.ShowMaterialEmpty();   // 前面已經加過的空畫面
                return;
            }

            var slotCode =
    (slot as SlotViewModel)?.SlotCode ??
    (slot as StorageSlotViewModel)?.SlotCode;

            if (material.Kind == MaterialKind.Electrode)
                _windowService.ShowElectrode(material.Electrode, material.Timeline, slotCode);
            else
                _windowService.ShowWorkpiece(material.Workpiece, material.Timeline, slotCode);
        }



        [RelayCommand]
        public void ShowDetail(string storageId)
        {
            // TODO: 傳入 storageId 給 DetailControl，如果要的話
            var overviewVM = new StorageDetailViewModel(this); // 傳入自己當 parent
            var overviewView = new StorageDetailControl
            {
                DataContext = overviewVM // 這一步非常重要！
            };
            CurrentStorageView = overviewView;
        }
        [RelayCommand]
        public void ShowOverview()
        {
            var overviewVM = new StorageOverviewViewModel(this); // 傳入自己當 parent
            var overviewView = new StorageOverviewControl
            {
                DataContext = overviewVM // 這一步非常重要！
            };
            CurrentStorageView = overviewView;
        }

        [RelayCommand]
        public void ShowMachineDetail()
        {
            // TODO: 傳入 storageId 給 DetailControl，如果要的話
            var overviewVM = new MachineDetailViewModel(this); // 傳入自己當 parent
            var overviewView = new MachineDetailControl
            {
                DataContext = overviewVM // 這一步非常重要！
            };
            CurrentWorkingZoneView = overviewView;
        }
        [RelayCommand]
        public void ShowMachineOverview()
        {
            var overviewVM = new MachineOverviewViewModel(this); // 傳入自己當 parent
            var overviewView = new MachineOverviewControl
            {
                DataContext = overviewVM // 這一步非常重要！
            };
            CurrentWorkingZoneView = overviewView;
        }
        [RelayCommand]
        private void OpenRobotWindow()
        {
            var win = new ShowRobotWindow();
            var owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            if (owner != null) win.Owner = owner;

            // 需要傳當前選擇的機器人資料時：
            // win.DataContext = new ShowRobotViewModel { DisplayData = SelectedRobotDisplayData };

            win.ShowDialog();
        }


        // 你原本就有 SelectedSlot / 或 Current slot，這裡用 IHasMaterial 代表
        public IHasMaterial? SelectedSlot { get; set; }

        // 顯示在按鈕上的字：電極：WRP20250512 / 工件：ABC-001（可依需求調整）
        public string CurrentMaterialLabel
        {
            get
            {
                var m = SelectedSlot?.Material;
                if (m == null) return "—";
                return m.Kind == MaterialKind.Electrode
                    ? $"電極：{(m.Electrode?.No ?? m.Electrode?.Name ?? "—")}"
                    : $"工件：{(m.Workpiece?.No ?? m.Workpiece?.Name ?? "—")}";
            }
        }

        // 讓 UI 能更新文字（當 SelectedSlot/Material 改變時請 Raise）
        public void NotifyCurrentMaterialChanged()
        {
            OnPropertyChanged(nameof(CurrentMaterialLabel));
        }

        [RelayCommand]
        private void ShowCurrentMaterialInfo()
        {
            var m = SelectedSlot?.Material;

            // 👉 沒資料就用一筆假資料打開
            if (m == null)
            {
                var demoElec = new ElectrodeModel
                {
                    No = "E-TEST-001",
                    Name = "示範電極"
                    // 其他屬性依你的模型可再補
                };
                _windowService.ShowMaterialInformation(demoElec, Array.Empty<TimelineItemModel>());
                return;
            }

            var tl = m.Timeline ?? Enumerable.Empty<TimelineItemModel>();

            if (m.Kind == MaterialKind.Electrode && m.Electrode != null)
                _windowService.ShowMaterialInformation(m.Electrode, tl);
            else if (m.Kind == MaterialKind.Workpiece && m.Workpiece != null)
                _windowService.ShowMaterialInformation(m.Workpiece, tl);
        }
    }
}
