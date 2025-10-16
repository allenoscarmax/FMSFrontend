using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging; // ← 新增
using CommunityToolkit.Mvvm.Messaging.Messages; // ← 新增：Message 型別
using ControlzEx.Standard;
using FMSFrontend.Controls;
using FMSFrontend.Extensions;
using FMSFrontend.Helpers;
using FMSFrontend.Models;
using FMSFrontend.Services;
using FMSFrontend.ViewModels.Windows;
using FMSFrontend.Views;
using IniFile;
using OSCARMAXFMS_V3.DBmodels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json; // ← 新增：JsonElement
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls; // 放在你的 ViewModel 上方
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace FMSFrontend.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        // ✅ 啟動時一次性並行抓取必要資料
        private async Task InitializeDataAsync()
        {
            try
            {
                var t1 = FetchAsrsParametersAsync();                               // 1) ASRS 
                var t2 = FetchProductionLinesAsync();                                  // 2) ProductionLines
                var t3 = FetchMachinesDataAsync();                                 // 3) Machine/DB_GetAllMachines
                var t4 = FetchAllCommandScheduleAsync();                           // 4) CommandScheduler/GetAllCommandSchedule
                var t5 = FetchMachineDataAsync(0);                                 // 5) Machine/GetMachineData/0

                await Task.WhenAll(t1, t2, t3, t4, t5);
            }
            catch
            {
                // 啟動期允許忽略暫時性錯誤，後續輪詢或手動刷新會再補上
            }
        }
        // 新增：封裝 ASRS 資料抓取（手臂/ASRS參數）
        private async Task FetchAsrsParametersAsync()
        {
            var list = await _httpService.GetJsonAsync<List<ASRSParameter>>("ASRS/GetASRSParameter");
            if (list == null || list.Count == 0) return;

            // 以目前索引綁定顯示單一機器人（若多台）
            _robotActionNum = list.Count;
            _robotActionIndex = Math.Clamp(_robotActionIndex, 1, Math.Max(1, _robotActionNum));
            var idx = _robotActionIndex - 1;
            var p = list[idx];

            // 更新機器人區塊
            if (Robot == null) Robot = new Robot();
            Robot.Name = $"機器人{(p.RobotNumber?.ToString() ?? string.Empty)}";
            Robot.CurrentLocation = p.RobotPosition ?? "-";
            Robot.CurrentAction = p.RobotDoingNow ?? "-";
            Robot.NextAction = p.RobotDoingNext ?? "-";
            Robot.IsMultipleRobotVisible = list.Count > 1;
            Robot.SelectedRobotIndexDisplay = $"{_robotActionIndex} / {_robotActionNum}";

            // 同步派工狀態與摘要/告警
            IsDispatch = p.DispatchSwitch;
            SDispatchText = IsDispatch ? "派工中" : "派工啟動";
            IsIdle = !p.IsRobotBusy;

            IsAlarm = p.IsRobotError || p.BattAlarm || p.AsrsProcessWarning;
            SummaryMessage = IsAlarm
                ? (string.IsNullOrWhiteSpace(p.RobotAlarmMessage) ? "機器人異常" : p.RobotAlarmMessage!)
                : "系統正常運作";

            // 廣播給訂閱頁面
            WeakReferenceMessenger.Default.Send(new AsrsParametersUpdatedMessage(list));
        }

        // 2) Storage/DB_GetAllStorageData → 產線總覽
        private async Task FetchProductionLinesAsync()
        {
            var json = await _httpService.GetJsonAsync<JsonElement>("Storage/DB_GetAllStorageData");
            _storageDataJson = json;





            WeakReferenceMessenger.Default.Send(new StorageDataUpdatedMessage(json));

        }

        // 3) Machine/DB_GetAllMachines → 產線總覽
        private async Task FetchMachinesDataAsync()
        {
            var json = await _httpService.GetJsonAsync<JsonElement>("Machine/DB_GetAllMachines");
            _machinesJson = json;
            WeakReferenceMessenger.Default.Send(new MachinesDataUpdatedMessage(json));
        }

        // 4) CommandScheduler/GetAllCommandSchedule → 整場總覽任務工作條
        private async Task FetchAllCommandScheduleAsync()
        {
            var json = await _httpService.GetJsonAsync<JsonElement>("CommandScheduler/Robot/DB_GetAllRobots");
            _commandScheduleJson = json;
            WeakReferenceMessenger.Default.Send(new CommandScheduleUpdatedMessage(json));
        }

        // 5) Machine/GetMachineData/{line} → 設備總覽機台資訊
        private async Task FetchMachineDataAsync(int line)
        {
            var route = $"Machine/GetMachineData/{line}";
            var json = await _httpService.GetJsonAsync<JsonElement>(route);
            _machineOverviewJson = json;
            WeakReferenceMessenger.Default.Send(new MachineOverviewDataUpdatedMessage(json));
        }
        // ====== 型別化訊息：讓各頁面可訂閱接收資料 ======
        // 新增：啟動時取得的 JSON 暫存
        private JsonElement _storageDataJson;
        private JsonElement _machinesJson;
        private JsonElement _commandScheduleJson;
        private JsonElement _machineOverviewJson;
        public sealed class StorageDataUpdatedMessage : ValueChangedMessage<JsonElement>
        {
            public StorageDataUpdatedMessage(JsonElement value) : base(value) { }
        }
        public sealed class MachinesDataUpdatedMessage : ValueChangedMessage<JsonElement>
        {
            public MachinesDataUpdatedMessage(JsonElement value) : base(value) { }
        }
        public sealed class CommandScheduleUpdatedMessage : ValueChangedMessage<JsonElement>
        {
            public CommandScheduleUpdatedMessage(JsonElement value) : base(value) { }
        }
        public sealed class MachineOverviewDataUpdatedMessage : ValueChangedMessage<JsonElement>
        {
            public MachineOverviewDataUpdatedMessage(JsonElement value) : base(value) { }
        }
        public sealed class RefreshPageMessage : ValueChangedMessage<string>
        {
            public RefreshPageMessage(string currentPageKey) : base(currentPageKey) { }
        }

        // 新增：ASRS 參數更新訊息
        public sealed class AsrsParametersUpdatedMessage : ValueChangedMessage<IReadOnlyList<ASRSParameter>>
        {
            public AsrsParametersUpdatedMessage(IReadOnlyList<ASRSParameter> value) : base(value) { }
        }
    }
}
