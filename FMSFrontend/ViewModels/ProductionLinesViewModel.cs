using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FMSFrontend.Views;
using FMSFrontend.Models;
using FMSFrontend.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using FMSFrontend.ViewModels.Production;
using System.Collections.ObjectModel;
using System.Windows.Media.Animation;
using System.Windows;

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

        public ProductionLinesViewModel()
        {
            // 預設載入總覽畫面
            ShowOverview();
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


    }
}
