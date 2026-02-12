using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Extensions;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;
using FMSFrontend.Interfaces;
using FMSFrontend.Models;
using FMSFrontend.Services;
using FMSFrontend.ViewModels.Factory;
using FMSFrontend.ViewModels.Windows;
using FMSFrontend.Views;
using MahApps.Metro.Controls;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection.PortableExecutable;
using System.Security.Policy;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FMSFrontend.ViewModels
{
    public partial class MachineCncViewModel : ObservableObject
    {
        //private readonly MachineOverviewMainViewModel _parent;
        // === Services ===
        private readonly IWindowService _windowService;
        private readonly IHttpService _httpService;
        private readonly IPlcService _plcService;
        private readonly IAuthorizationService _authorizationService;

        // === Singleton ===
        private readonly StationStore _store = new();
        private StationModel Station => _store.Station;

        private CancellationTokenSource? _currentUpdateCts; // 取消目前更新的 CancellationTokenSource

        DispatcherTimer _timer;
        // 對 UI 綁定的資料封裝 (改為屬性，供 WPF Binding 使用)
        public DisplayData displayData { get; } = new();

        [ObservableProperty]
        private string machineImagePath = string.Empty; // 機台圖片路徑

        [ObservableProperty]
        private string machineName = string.Empty; // 機台名稱

        public MachineCncViewModel(IWindowService windowService,
        IHttpService httpService,
        IPlcService plcService,
        IAuthorizationService authorizationService,
        StationStore stationStore)
        {
            _windowService = windowService;
            _httpService = httpService;
            _plcService = plcService;
            _store = stationStore;
            _authorizationService = authorizationService;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _timer.Tick += (_, __) => RefreshFromStore();

            // 初始化一次顯示資料
            RefreshFromStore();
        }

        // 由外部把「要顯示哪一台機」與「父 VM」丟進來
        public void Initialize(MachineOverviewCard machine, MachineOverviewMainViewModel parent)
        {
            MachineImagePath = machine.MachineImagePath;
            MachineName = machine.MachineName;
            RefreshFromStore();
            _timer.Start();
        }


        int Cnt = 0;
        void RefreshFromStore() //更新
        {
        }
        public void OnPageActivated() //開機
        {

        }
        public void OnPageDeactivated() //關機
        {
            _timer.Stop();
            _timer = null!;
        }
        [RelayCommand]
        private async Task OpenDoorClick() //開門
        {
            try
            {
                if (!_authorizationService.RequireLoginAndWriteOperation(31)) return;
                // 取消前一次仍在執行的更新,逾時設定1秒
                bool ok = await _plcService.ASE_OpenDoorAsync();
                if (!ok)
                    throw new System.Exception("API回傳失敗");
            }
            catch
            {
                new DialogMessageWindow("API發送失敗").ShowDialog();
            }
        }

        [RelayCommand]
        private async Task OpenDoorLightClick(string open) //  開門燈
        {
            try
            {
                if (!_authorizationService.RequireLoginAndWriteOperation(open == "True" ? 32 : 33)) return;
                // 取消前一次仍在執行的更新,逾時設定1秒
                bool ok = await _plcService.ASE_OpenDoorLightAsync(open == "True");
                if (!ok)
                    throw new System.Exception("API回傳失敗");
            }
            catch
            {
                new DialogMessageWindow("API發送失敗").ShowDialog();
            }
        }

        [RelayCommand]
        private async Task OpenChuckClick(string open) //  開夾頭
        {
            try
            {
                if (!_authorizationService.RequireLoginAndWriteOperation(open == "True" ? 34 : 35)) return;
                // 取消前一次仍在執行的更新,逾時設定1秒
                bool ok = await _plcService.ASE_OpenChuckAsync(open == "True");
                if (!ok)
                    throw new System.Exception("API回傳失敗");
            }
            catch
            {
                new DialogMessageWindow("API發送失敗").ShowDialog();
            }
        }

        [RelayCommand]
        private async Task OutcomingPartClick(string open)
        {
            try
            {
                // 取消前一次仍在執行的更新,逾時設定1秒
                bool ok = await _plcService.ASE_Require_OutcomingPartAsync(open == "True");
                if (!ok)
                    throw new System.Exception("API回傳失敗");
            }
            catch
            {
                new DialogMessageWindow("API發送失敗").ShowDialog();
            }
        }

        [RelayCommand]
        private async Task IncomingPartClick(string open)
        {
            try
            {
                if (!_authorizationService.RequireLoginAndWriteOperation(36)) return;
                // 取消前一次仍在執行的更新,逾時設定1秒
                bool ok = await _plcService.ASE_Require_IncomingPartAsync(open == "True");
                if (!ok)
                    throw new System.Exception("API回傳失敗");
            }
            catch
            {
                new DialogMessageWindow("API發送失敗").ShowDialog();
            }
        }

        public partial class DisplayData : ObservableObject
        {
            [ObservableProperty] private string machineImagePath = ""; //機台圖片路徑
            [ObservableProperty] private string air_error = "";
            [ObservableProperty] private string doorisOpen = ""; // 門是否被打開 ON 打開 OFF 關閉中
            [ObservableProperty] private string rFIDisPolarization = ""; // ON 表示回到安全位置，OFF 表示不在安全位置
            [ObservableProperty] private string workpieceOnAssemblyStation = ""; // ON 表示有工件，OFF 表示無工件
            [ObservableProperty] private string notification_IncomingPart = ""; // ON=手臂可取工件，OFF=無工件要進來
            [ObservableProperty] private string notification_WaitingWorkpieceReturn = ""; // ON=手臂可放工件，OFF=無工件要回去
            [ObservableProperty] private string alarm = ""; // ON=有警報，OFF=無警報
            [ObservableProperty] private string require_IncomingPart = ""; // UI要求進工件 (UI→後台)
            [ObservableProperty] private string require_OutcomingPart = ""; // UI要求出工件 (UI→後台)
            [ObservableProperty] private string notification_Doorislocked = ""; // PLC回饋 門已鎖好
        }
    }
}
