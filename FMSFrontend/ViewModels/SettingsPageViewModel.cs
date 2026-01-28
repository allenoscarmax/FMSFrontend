using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Extensions;   // ← PeriodWindow / PeriodWindowArgs / PeriodSelectionResult / ScheduleMode
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Interfaces;
using FMSFrontend.Services;
using FMSFrontend.ViewModels.Windows;
using FMSFrontend.Views.Windows;
using IniFile;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Collections.Generic;
using System.Windows.Media.Animation;
using System.Security;
using System.Windows.Navigation;
using MongoDB.Bson.Serialization.Attributes;

namespace FMSFrontend.ViewModels
{

    public partial class SettingsPageViewModel : ObservableObject
    {


        // ===== 顯示用（你的 XAML 綁在這個上面）=====
        [ObservableProperty] private string periodDisplay = "不設定";

        // 新增：綁定系統 IP 的輸入欄位
        [ObservableProperty] private string serverIp = string.Empty;
        [ObservableProperty] private string robotMaintenanceMsg = "";
        // ===== 目前的設定（當作下一次開窗的初始值）=====
        [ObservableProperty] private ScheduleMode selectedMode = ScheduleMode.None;

        public ObservableCollection<int> Weekly { get; } = new();   // 1~7、可負數
        public ObservableCollection<int> Monthly { get; } = new();   // 1~31、0=last、可負數
        [ObservableProperty] private int hour = 9;   // 0~23
        [ObservableProperty] private int minute = 41;  // 0~59
        [ObservableProperty] private bool isPm = false;

        // ===== 開窗指令 ── 綁到你的 Button =====
        public IRelayCommand OpenSetPeriodDialogCommand { get; }

        private readonly IWindowService _windowService;
        private readonly IHttpService _httpService;

        private readonly IMachinesService _machinesService;
        private readonly IRobotService _robotService;
        private readonly IDevicesService _devicesService;
        private readonly IWorkerService _workerService;
        private readonly IAppointmentMaintenanceService _appointmentMaintenanceService;
        private readonly IMongoDBService _mongoDBService;
        private  readonly IAuthorizationService _authorizationService;
        public SettingsPageViewModel(IWindowService windowService,
            IHttpService httpService,
            IMachinesService machinesService,
            IRobotService robotService,
            IDevicesService devicesService,
            IWorkerService workerService,
            IAppointmentMaintenanceService appointmentMaintenanceService,
            IMongoDBService mongoDBService,
            IAuthorizationService authorizationService
            )
        {
            _windowService = windowService;
            _httpService = httpService;

            _machinesService = machinesService;
            _robotService = robotService;
            _devicesService = devicesService;
            _workerService = workerService;
            _appointmentMaintenanceService = appointmentMaintenanceService;
            _mongoDBService = mongoDBService;
            _authorizationService = authorizationService;

            OpenSetPeriodDialogCommand = new RelayCommand(OpenPeriodDialog);

            // 初始化 IP 顯示（優先用 IHttpService 的 ServerIp）
            ServerIp = _httpService.ServerIp ?? string.Empty;

            // 初始化設定值（原本內容保留）
            AvailableLanguages = new ObservableCollection<string> { "繁體中文", "English", "日本語" };
            SelectedLanguage = AvailableLanguages[0];

            AvailableThemes = new ObservableCollection<string> { "Light", "Dark", "System Default" };
            SelectedTheme = AvailableThemes[0];

            AvailablePermissions = new ObservableCollection<string> { "工作人員", "專家" };
            SelectedPermission = AvailablePermissions[0];

            // 初始化權限選項
            AvailablePermissions = new ObservableCollection<string> { "工作人員", "專家" };
            SelectedPermission = AvailablePermissions[0];

            /*
            MachineList = new ObservableCollection<MachineInfo>{
                new MachineInfo { MachineId="EDM1", MachineName="EDM1", MachineType="EDM1", IpAddress="192.168.21.232", Port=13101, AssetNo="-", Owner="OscarMax" },
                new MachineInfo { MachineId="EDM2", MachineName="EDM2", MachineType="EDM2", IpAddress="192.168.21.233", Port=13102, AssetNo="-", Owner="OscarMax" },
                new MachineInfo { MachineId="EDM3", MachineName="EDM3", MachineType="EDM3", IpAddress="192.168.21.234", Port=13103, AssetNo="-", Owner="OscarMax" },
                new MachineInfo { MachineId="EDM4", MachineName="EDM4", MachineType="EDM4", IpAddress="192.168.21.235", Port=13104, AssetNo="-", Owner="OscarMax" },
                new MachineInfo { MachineId="EDM5", MachineName="EDM5", MachineType="EDM5", IpAddress="192.168.21.236", Port=13105, AssetNo="-", Owner="OscarMax" },
            };
            */
            _machinesView = CollectionViewSource.GetDefaultView(MachineList);
            _machinesView.Filter = FilterMachine;        // 設定一次即可
            /*
            RobotList = new ObservableCollection<RobotInfo>{
    new RobotInfo { RobotId="Robot1", RobotName="Robot1", RobotType="Fanuc", IpAddress="192.168.21.6", Owner="OscarMax" },
};
            */
            _robotsView = CollectionViewSource.GetDefaultView(RobotList);
            _robotsView.Filter = FilterRobot;        // 設定一次即可
            /*
            // Device 假資料（依你的截圖）
            DeviceList = new ObservableCollection<DeviceInfo>
            {
                new() { DeviceId="Delta_IO",      DeviceName="Delta_IO",      IpAddress="192.168.21.239", Port=10001, AssetNo="-", Owner="OscarMax" },
                new() { DeviceId="Balluff_RFID",  DeviceName="Balluff_RFID",  IpAddress="192.168.21.236", Port=10001, AssetNo="-", Owner="OscarMax" },
                new() { DeviceId="ESL",           DeviceName="ESL",           IpAddress="192.168.21.238", Port=10001, AssetNo="-", Owner="OscarMax" },
            };
            */
            // 建立 View + Filter
            _devicesView = CollectionViewSource.GetDefaultView(DeviceList);
            _devicesView.Filter = FilterDevice;

            _workerListView = CollectionViewSource.GetDefaultView(WorkerList);
            _workerListView.Filter = FilterWorker;
            INIFile ini = new INIFile(AppDomain.CurrentDomain.BaseDirectory + "\\Basesitting.ini");
            RobotMaintenanceMsg = "上次日期" + ini.Read("Prarm", "RobotMaintenanceMsg");
        }
        // 這就是缺少的屬性，用來綁定 Tab 切換
        [ObservableProperty] private int selectedTabIndexParameter;

        [ObservableProperty] private int selectedSubTabIndexParameter;

        partial void OnSelectedTabIndexParameterChanged(int value) //主頁籤切換
        {
            switch (value)
            {
                case 0: //設定ip

                    break;
                case 1: //系統還原
                    break;
                case 2: //設備資訊
                    selectedSubTabIndexParameter = 0;
                    OnSelectedSubTabIndexParameterChanged(0); // 修正拼字錯誤
                    break;
                case 3:
                    _ = WorkerRefresh();
                    break;
                case 4: //保養 
                    int[] week = { 1, 3, 5, 7 };
                    PeriodDisplay = BuildPeriodDisplay(ScheduleMode.Weekly, week, [0], 10, 12);
                    // _ = AppointmentMaintenanceRefresh();
                    break;
            }
        }
        // 修正：CommunityToolkit 產生的部分方法必須是 partial void；內部以 fire-and-forget 呼叫非同步刷新
        partial void OnSelectedSubTabIndexParameterChanged(int value) //設備資訊子頁籤切換
        {
            switch (value)
            {
                case 0:
                    _ = MachinesRefresh().ContinueWith(t =>
                    {
                        if (MachineList.Count > 0) SelectedMachine = MachineList[0];
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                    break;
                case 1:
                    _ = RobotRefresh().ContinueWith(t =>
                    {
                        if (RobotList.Count > 0) SelectedRobot = RobotList[0];
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                    break;
                case 2:
                    _ = DeviceRefresh().ContinueWith(t =>
                    {
                        if (DeviceList.Count > 0) SelectedDevice = DeviceList[0];
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                    break;
            }
        }

        private async Task MachinesRefresh() //更新Machine資訊
        {
            try
            {
                List<MachinesDto> dtos = await _machinesService.GetAllMachinesAsync() ?? new List<MachinesDto>();
                MachineList.Clear();
                foreach (var dto in dtos)
                {
                    MachineList.Add(
                    new MachineInfo
                    {
                        MachineId = dto._id,
                        MachineNo = dto.machineNumber.ToString(),
                        MachineName = dto.machineName,
                        MachineType = dto.machineCode,
                        IpAddress = dto.ip,
                        Port = dto.port,
                        Owner = dto.setupUser
                    });
                }

            }
            catch
            {
            }
        }
        private async Task RobotRefresh() //更新Robot資訊
        {
            try
            {
                List<RobotDto> Dtos = await _robotService.DB_GetAllRobotsAsync() ?? new List<RobotDto>();
                RobotList.Clear();
                foreach (var dto in Dtos)
                {
                    RobotList.Add(
                    new RobotInfo
                    {
                        RobotId = dto._id,
                        RobotNo = "1",
                        RobotName = dto.robotName,
                        RobotType = "FANUC",
                        IpAddress = dto.robot_IP,
                        Owner = dto.setupUser
                    });
                }

            }
            catch { }
        }
        private async Task DeviceRefresh() //更新Device資訊
        {
            try
            {
                List<DevicesDto> dtos = await _devicesService.GetAllDevicesAsync() ?? new List<DevicesDto>();
                DeviceList.Clear();
                foreach (var dto in dtos)
                {
                    DeviceList.Add(
                    new DeviceInfo
                    {
                        DeviceId = dto.Id,
                        DeviceNo = dto.DeviceNumber.ToString(),
                        DeviceName = dto.DeviceName,
                        IpAddress = dto.DeviceIP,
                        Port = dto.DevicePort,
                        Owner = dto.SetupUser
                    });
                }
            }
            catch { }
        }
        private async Task WorkerRefresh() //更新Worker資訊
        {
            try
            {
                List<WorkerDto> dtos = await _workerService.GetAllWorkerAsync() ?? new();

                WorkerList.Clear();
                foreach (var dto in dtos)
                {
                    WorkerList.Add(
                    new WorkerInfo
                    {
                        Id = dto.Id,
                        WorkerNumber = dto.WorkerNumber,
                        WorkerName = dto.WorkerName,
                        Psssword = dto.Password
                    });
                }
            }
            catch
            {
            }
        }
        [RelayCommand]
        private async Task EditMachine()
        {
            if (SelectedMachine == null)
            {
                _windowService.ShowMessage("請先選擇要編輯的機台");
                return;
            }

            // 動態建立可編輯欄位：IP / Port 可編輯，其餘只顯示
            var items = new List<EditGridViewModel.EditGridInfo>
            {
                new() { Title = "機台編號", Data = SelectedMachine.MachineId, Enable = false },
                new() { Title = "名稱", Data = SelectedMachine.MachineName, Enable = false },
                new() { Title = "類型", Data = SelectedMachine.MachineType, Enable = false },
                new() { Title = "IP", Data = SelectedMachine.IpAddress, Enable = true },
                new() { Title = "Port", Data = SelectedMachine.Port, Enable = true }
            };
            var win = new EditGridWindow(items, "編輯機台連線參數");
            if (win.ShowDialog() == true)
            {
                try
                {
                    // 使用顯示集合找出輸入後的值
                    string newIp = win.ViewModel.Display.First(x => x.Title == "IP").Data.Trim();
                    string newPort = win.ViewModel.Display.First(x => x.Title == "Port").Data.Trim();

                    // 取得所有機台，找出欲更新的 DTO
                    var all = await _machinesService.GetMachinesByMachineNameAsync(SelectedMachine.MachineName) ?? new();
                    var dto = all.FirstOrDefault(m => m._id == SelectedMachine.MachineId);
                    if (dto == null)
                    {
                        _windowService.ShowMessage("找不到原始機台資料，更新失敗");
                        return;
                    }

                    dto.ip = newIp;
                    dto.port = newPort;
                    if (!_authorizationService.RequireLoginAndWriteOperation(22))
                        return;
                    bool ok = await _machinesService.UpdateMachinesDataAsync(dto);
                    if (!ok)
                    {
                        _windowService.ShowMessage("機台更新失敗，請稍後再試");
                        return;
                    }

                    StatusMessage = "✅ 機台參數已更新";
                }
                catch
                {
                    _windowService.ShowMessage("更新過程發生錯誤");
                }
                // 重新載入機台列表
                await MachinesRefresh();
            }
        }
        [ObservableProperty] private RobotInfo? selectedRobot;
        [RelayCommand]
        private async Task EditRobot()
        {
            if (SelectedRobot == null)
            {
                _windowService.ShowMessage("請先選擇要編輯的機械手臂");
                return;
            }

            // 動態建立可編輯欄位：僅 IP 可編輯
            var items = new List<EditGridViewModel.EditGridInfo>
            {
                new() { Title = "機械手臂編號", Data = SelectedRobot.RobotId, Enable = false },
                new() { Title = "名稱", Data = SelectedRobot.RobotName, Enable = false },
                new() { Title = "類型", Data = SelectedRobot.RobotType, Enable = false },
                new() { Title = "IP", Data = SelectedRobot.IpAddress, Enable = true }
            };
            var win = new EditGridWindow(items, "編輯機械手臂連線參數");
            if (win.ShowDialog() == true)
            {
                // 使用顯示集合找出輸入後的值
                string newIp = win.ViewModel.Display.First(x => x.Title == "IP").Data.Trim();

                // 取得所有機械手臂，找出欲更新的 DTO
                var all = await _robotService.DB_GetAllRobotsAsync() ?? new();
                var dto = all.FirstOrDefault(r => r._id == SelectedRobot.RobotId);
                if (dto == null)
                {
                    _windowService.ShowMessage("找不到原始機械手臂資料，更新失敗");
                    return;
                }

                dto.robot_IP = newIp;
                if (!_authorizationService.RequireLoginAndWriteOperation(23))
                    return;
                
                bool ok = await _robotService.DB_UpdateRobotDataAsync(dto);
                if (!ok)
                {
                    _windowService.ShowMessage("機械手臂更新失敗，請稍後再試");
                    return;
                }

                StatusMessage = "✅ 機械手臂參數已更新";

                // 重新載入機械手臂列表
                await RobotRefresh();
            }
        }
        [RelayCommand]
        private async Task EditDevice()
        {
            if (SelectedDevice == null)
            {
                _windowService.ShowMessage("請先選擇要編輯的裝置");
                return;
            }

            // 動態建立可編輯欄位：IP / Port 可編輯，其餘只顯示
            var items = new List<EditGridViewModel.EditGridInfo>
            {
                new() { Title = "裝置編號", Data = SelectedDevice.DeviceId, Enable = false },
                new() { Title = "名稱", Data = SelectedDevice.DeviceName, Enable = false },
                new() { Title = "IP", Data = SelectedDevice.IpAddress, Enable = true },
                new() { Title = "Port", Data = SelectedDevice.Port, Enable = true }
            };
            var win = new EditGridWindow(items, "編輯裝置連線參數");
            if (win.ShowDialog() == true)
            {
                // 使用顯示集合找出輸入後的值
                string newIp = win.ViewModel.Display.First(x => x.Title == "IP").Data.Trim();
                string newPort = win.ViewModel.Display.First(x => x.Title == "Port").Data.Trim();

                // 取得所有裝置，找出欲更新的 DTO
                var all = await _devicesService.GetAllDevicesAsync() ?? new();
                var dto = all.FirstOrDefault(d => d.Id == SelectedDevice.DeviceId);
                if (dto == null)
                {
                    _windowService.ShowMessage("找不到原始裝置資料，更新失敗");
                    return;
                }

                dto.DeviceIP = newIp;
                dto.DevicePort = newPort;
                if (!_authorizationService.RequireLoginAndWriteOperation(24))
                    return;
                bool ok = await _devicesService.UpdateDeviceDataAsync(dto);
                if (!ok)
                {
                    _windowService.ShowMessage("裝置更新失敗，請稍後再試");
                    return;
                }
                
                StatusMessage = "✅ 裝置參數已更新";

                // 重新載入裝置列表
                await DeviceRefresh();
            }
        }
        [RelayCommand]
        private async Task AddWorker() // 新增使用者
        {
            var existingWorkers = WorkerList
                .Select(w => new LoginInfo { Number = w.WorkerNumber,  Name = w.WorkerName, Password = w.Psssword })
                .ToList();

            var result = _windowService.ShowAddWorkerWindow(existingWorkers, "","");
            if (result == null)
                return;

            try
            {
                _authorizationService.WriteOperation(25);
                bool ok = await _workerService.InsertNewWorkerDataAsync(new WorkerDto
                {
                    WorkerNumber = result.Number,    
                    WorkerName = result.Name,
                    Password = result.Password
                });

                if (!ok)
                {
                    _windowService.ShowMessage("新增使用者失敗，請稍後再試");
                }
            }
            catch
            {
                _windowService.ShowMessage("新增使用者時發生錯誤");
            }

            await WorkerRefresh();
        }

        [RelayCommand]
        private async Task EditWorker() // 編輯使用者
        {
            var existingWorkers = WorkerList
                .Select(w => new LoginInfo { Id = w.Id, Name = w.WorkerNumber, Password = w.Psssword })
                .ToList();

            if (SelectedWorker == null || string.IsNullOrWhiteSpace(SelectedWorker.WorkerNumber))
            {
                _windowService.ShowMessage("請先選擇要編輯的使用者");
                return;
            }

            var result = _windowService.ShowAddWorkerWindow(existingWorkers, SelectedWorker.WorkerNumber, SelectedWorker.WorkerName);
            if (result == null)
                return;

            try
            {
                if (!_authorizationService.RequireLoginAndWriteOperation(25))
                    return;
                bool ok = await _workerService.UpdateWorkerDataAsync(new WorkerDto
                {
                    Id = SelectedWorker.Id,
                    WorkerNumber = result.Number,     
                    WorkerName = result.Name,
                    Password = result.Password
                });

                if (!ok)
                {
                    _windowService.ShowMessage("使用者更新失敗，請稍後再試");
                }
            }
            catch
            {
                _windowService.ShowMessage("使用者更新時發生錯誤");
            }

            await WorkerRefresh();
        }

        [RelayCommand]
        private async Task RemoveWorker()
        {
            try
            {
                if (SelectedWorker.WorkerNumber == null || SelectedWorker.WorkerNumber == "")
                {
                    _windowService.ShowMessage("請選擇要刪除的使用者");
                    return;
                }
                if (!_authorizationService.RequireLoginAndWriteOperation(26))
                    return;
                bool ok = await _workerService.DeleteWorkerDataByIdAsync(SelectedWorker.Id);
                if (!ok)
                {
                    _windowService.ShowMessage("使用者刪除失敗，請稍後再試");
                }
                await WorkerRefresh();
            }
            catch { }
        }
        private async Task AppointmentMaintenanceRefresh()
        {
            try
            {
                List<AppointmentMaintenanceDto> dtos = await _appointmentMaintenanceService.GetAllAppointmentMaintenanceAsync() ?? new List<AppointmentMaintenanceDto>();
                //AppointmentMaintenanceList.Clear();
                if (!dtos[0].IsEnabled)
                {
                    PeriodDisplay = "不設定";
                }
                else
                {
                    switch (dtos[0].Type)
                    {
                        case "daily":
                            PeriodDisplay = BuildPeriodDisplay(ScheduleMode.Daily, [0], [0], dtos[0].Hour, dtos[0].Minute);
                            break;
                        case "weekly":
                            PeriodDisplay = BuildPeriodDisplay(ScheduleMode.Weekly, dtos[0].DayValues.ToArray(), [0], dtos[0].Hour, dtos[0].Minute);
                            break;
                        case "monthly":
                            PeriodDisplay = BuildPeriodDisplay(ScheduleMode.Monthly, [0], dtos[0].DayValues.ToArray(), dtos[0].Hour, dtos[0].Minute);
                            break;
                        default:
                            PeriodDisplay = BuildPeriodDisplay(ScheduleMode.None, [0], [0], dtos[0].Hour, dtos[0].Minute);
                            break;
                    }
                }
            }
            catch { }
        }

        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private string busyMessage = "處理中...";
        private bool CanBackupSystem() => !IsBusy;

        [RelayCommand(CanExecute = nameof(CanBackupSystem))]
        private async Task BackupSystem()
        {
            IsBusy = true;
            BusyMessage = "系統備份中，請稍候...";
            try
            {
                if (!_authorizationService.RequireLoginAndWriteOperation(20))
                {
                    return;
                } 
                var ok = await _mongoDBService.BackupDatabaseAsync();

                _windowService.ShowMessage(ok ? "系統備份成功" : "系統備份失敗");
            }
            catch (Exception ex)
            {
                _windowService.ShowMessage($"系統備份發生例外：{ex.Message}");
                // 建議也寫 log
            }
            finally
            {
                IsBusy = false;
                BackupSystemCommand.NotifyCanExecuteChanged();
            }
        }

        // === 設定屬性 ===
        [ObservableProperty]
        private ObservableCollection<string> availableLanguages;

        [ObservableProperty]
        private string selectedLanguage = "";

        [ObservableProperty]
        private ObservableCollection<string> availableThemes;

        [ObservableProperty]
        private string selectedTheme = "";

        [ObservableProperty]
        private bool enableNotifications = true;

        [ObservableProperty]
        private bool autoUpdate = true;

        [ObservableProperty]
        private string statusMessage = "設定尚未儲存";
        // === 權限相關屬性 ===
        [ObservableProperty]
        private ObservableCollection<string> availablePermissions;

        [ObservableProperty]
        private string selectedPermission = "";

        //MachineList
        public ObservableCollection<WorkerInfo> WorkerList { get; set; } = new();
        [ObservableProperty] private WorkerInfo selectedWorker = new();
        private readonly ICollectionView _workerListView;
        private string _workerSearchText = "";
        public string WorkerSearchText
        {
            get => _workerSearchText;
            set
            {
                // 由 ObservableObject 提供，會幫你 Raise PropertyChanged
                if (SetProperty(ref _workerSearchText, value))
                {
                    _workerListView?.Refresh(); // 每次變更就重新套過濾
                }
            }
        }

        //MachineList
        public ObservableCollection<MachineInfo> MachineList { get; set; } = new();
        private readonly ICollectionView _machinesView;
        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                // 由 ObservableObject 提供，會幫你 Raise PropertyChanged
                if (SetProperty(ref _searchText, value))
                {
                    _machinesView?.Refresh(); // 每次變更就重新套過濾
                }
            }
        }
        // ★ 新增：選取的機台（供編輯用）
        [ObservableProperty] private MachineInfo? selectedMachine;
        //RobotList
        public ObservableCollection<RobotInfo> RobotList { get; set; } = new();
        private readonly ICollectionView _robotsView;
        private string _robotSearchText = "";
        public string RobotSearchText
        {
            get => _robotSearchText;
            set
            {
                // 由 ObservableObject 提供，會幫你 Raise PropertyChanged
                if (SetProperty(ref _robotSearchText, value))
                {
                    _robotsView?.Refresh(); // 每次變更就重新套過濾
                }
            }
        }
        // ====== Device 區 ======

        // 1) 清單 + 視圖
        public ObservableCollection<DeviceInfo> DeviceList { get; set; } = new();
        private readonly ICollectionView _devicesView;

        // 2) 選取項目（可選）
        private DeviceInfo _selectedDevice = new DeviceInfo();
        public DeviceInfo SelectedDevice
        {
            get => _selectedDevice;
            set => SetProperty(ref _selectedDevice, value);
        }

        // 3) 搜尋文字（即時過濾）
        private string _deviceSearchText = "";
        public string DeviceSearchText
        {
            get => _deviceSearchText;
            set
            {
                if (SetProperty(ref _deviceSearchText, value))
                    _devicesView?.Refresh();
            }
        }

        private void OpenPeriodDialog()
        {
            if (!_authorizationService.RequireLogin()) 
                return;
            var win = new PeriodWindow
            {
                Owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
            };

            var vm = (PeriodWindowViewModel)win.DataContext;
            vm.ApplyInitial(SelectedMode, Weekly.ToArray(), Monthly.ToArray(), Hour, Minute, IsPm, "潤滑週期");

            if (win.ShowDialog() == true)
            {
                // ✅ 改用 ConfirmedMode + ConfirmedDays
                SelectedMode = vm.ConfirmedMode;

                Weekly.Clear();
                Monthly.Clear();
                if (SelectedMode == ScheduleMode.Weekly)
                    foreach (var v in vm.ConfirmedDays) Weekly.Add(v);
                else if (SelectedMode == ScheduleMode.Monthly)
                    foreach (var v in vm.ConfirmedDays) Monthly.Add(v);

                Hour = vm.Hour;
                Minute = vm.Minute;
                PeriodDisplay = BuildPeriodDisplay(SelectedMode, Weekly.ToArray(), Monthly.ToArray(), Hour, Minute);
            }
            _ = UpdataPeriod();
        }
        private async Task UpdataPeriod()
        {
            try
            {
                List<AppointmentMaintenanceDto> dtos = await _appointmentMaintenanceService.GetAllAppointmentMaintenanceAsync() ?? new List<AppointmentMaintenanceDto>();
                if (dtos.Count > 0)
                {
                    dtos[0].IsEnabled = SelectedMode != ScheduleMode.None;
                    if (SelectedMode != ScheduleMode.None)
                    {
                        dtos[0].Type = SelectedMode switch
                        {
                            ScheduleMode.Daily => "daily",
                            ScheduleMode.Weekly => "weekly",
                            ScheduleMode.Monthly => "monthly",
                            _ => string.Empty
                        };
                        if (SelectedMode == ScheduleMode.Weekly)
                        {
                            dtos[0].DayValues = Weekly.ToList();
                        }
                        else if (SelectedMode == ScheduleMode.Monthly)
                        {
                            dtos[0].DayValues = Monthly.ToList();
                        }
                        dtos[0].Hour = Hour;
                        dtos[0].Minute = Minute;
                    }
                    if (!_authorizationService.RequireLoginAndWriteOperation(27))
                        return;
                    bool ok = await _appointmentMaintenanceService.UpdateAppointmentMaintenanceAsync(dtos[0]);
                    if (!ok)
                    {
                        _windowService.ShowMessage("潤滑週期更新失敗，請稍後再試");
                    }
                }
            }
            catch { }
        }
        private static string BuildPeriodDisplay(ScheduleMode mode, int[] weekly, int[] monthly, int hour, int minute)
        {
            string time = $"{hour:00}:{minute:00}";
            if (mode == ScheduleMode.None)
            {
                return "不設定";
            }
            else if (mode == ScheduleMode.Daily)
            {
                return $"每日 {time} 進行潤滑";
            }
            else if (mode == ScheduleMode.Weekly)
            {
                string[] w = { "日", "一", "二", "三", "四", "五", "六" };
                string L(int v) => v > 0 ? w[v - 1] : "-" + w[-v - 1];
                return $"每週 {string.Join("、", weekly.Select(L))} {time} 進行潤滑";
            }
            else
            {
                string L(int v) => v == 0 ? "last" : v.ToString();
                return $"每月 {string.Join(",", monthly.Select(L))} 日 {time} 進行潤滑";
            }
        }



        // === 命令 ===
        [RelayCommand]
        private async Task SaveSettingsAsync()
        {
            // 模擬儲存邏輯，例如呼叫 API 或寫入本地設定檔
            await Task.Delay(500);
            StatusMessage = "✅ 設定已儲存於 " + DateTime.Now.ToString("HH:mm:ss");

            // 透過 WindowService 提示使用者
            _windowService.ShowMessage("設定儲存成功");
        }

        [RelayCommand]
        private void ResetSettings()
        {
            SelectedLanguage = AvailableLanguages[0];
            SelectedTheme = AvailableThemes[0];
            EnableNotifications = true;
            AutoUpdate = true;
            StatusMessage = "設定已重置";
            SelectedTabIndexParameter = 0; // 重置到第一個 Tab
            SelectedSubTabIndexParameter = 0; // 重置到第一個 Tab
        }
        // === 系統還原 ===
        [RelayCommand]
        private void RestoreSystem()
        {
            // TODO: 加入系統還原邏輯，例如清除資料、表單、日誌等
            StatusMessage = "⚠️ 系統已還原，所有資料已清除";
            _windowService.ShowMessage("系統還原完成");
        }

        // === 日誌重置 ===
        [RelayCommand]
        private void ResetLogs()
        {
            // TODO: 加入清除日誌邏輯
            StatusMessage = "🗑️ 系統日誌已清除";
            _windowService.ShowMessage("日誌重置完成");
        }

        // === 密碼重置 ===
        [RelayCommand]
        private void ResetPassword()
        {
            // TODO: 加入密碼重置邏輯
            StatusMessage = "🔐 權限密碼已重置";
            _windowService.ShowMessage("密碼重置完成");
        }

        // === 機械手臂維護完成 ===
        [RelayCommand]
        private void CompleteRobotMaintenance()
        {
            if (!_authorizationService.RequireLoginAndWriteOperation(21))
                return;
            INIFile ini = new INIFile(AppDomain.CurrentDomain.BaseDirectory + "\\Basesitting.ini");
            var today = DateTime.Today.ToString("yyyy/MM/dd");
            ini.Write("Prarm", "RobotMaintenanceMsg", today);
            RobotMaintenanceMsg = "上次維護時間: " + today;
            StatusMessage = "🤖 機械手臂維護已標記為完成";
            _windowService.ShowMessage("維護狀態已更新");
        }
        // === 設定IP ===
        [RelayCommand]
        private void SaveIP()
        {
            var ip = (ServerIp ?? string.Empty).Trim();
            if (!(IPAddress.TryParse(ip, out _) || ip == "localhost"))
            {
                StatusMessage = "❌ IP 位址格式不正確";
                _windowService.ShowMessage("IP 位址格式不正確，請輸入有效的 IPv4，例如：192.168.1.100");
                return;
            }
            //  try
            //  {
            _httpService.UpdateServerIp(ip);
            StatusMessage = "🤖 設定IP OK";
            _windowService.ShowMessage($"已設定 IP：{ip}");
            INIFile ini = new INIFile(AppDomain.CurrentDomain.BaseDirectory + "\\Basesitting.ini");
            ini.Write("Prarm", "IP", ip);
            /*  }
              catch (Exception ex)
              {
                  StatusMessage = "❌ 設定 IP 失敗";
                  _windowService.ShowMessage($"設定 IP 失敗：{ex.Message}");
              }
            */
        }
        private bool FilterWorker(object obj)
        {
            if (obj is not WorkerInfo m) return false;

            var q = (WorkerSearchText ?? string.Empty).Trim();
            if (q.Length == 0) return true;

            var cmp = StringComparison.OrdinalIgnoreCase;
            return (m.WorkerNumber?.Contains(q, cmp) ?? false)
                || (m.WorkerName?.Contains(q, cmp) ?? false);
        }

        private bool FilterMachine(object obj)
        {
            if (obj is not MachineInfo m) return false;

            var q = (SearchText ?? string.Empty).Trim();
            if (q.Length == 0) return true;

            var cmp = StringComparison.OrdinalIgnoreCase;
            return (m.MachineId?.Contains(q, cmp) ?? false)
                || (m.MachineName?.Contains(q, cmp) ?? false)
                || (m.MachineType?.Contains(q, cmp) ?? false)
                || (m.IpAddress?.Contains(q, cmp) ?? false)
                || m.Port.ToString().Contains(q, cmp);
        }
        private bool FilterRobot(object obj)
        {
            if (obj is not RobotInfo m) return false;

            var q = (RobotSearchText ?? string.Empty).Trim();
            if (q.Length == 0) return true;

            var cmp = StringComparison.OrdinalIgnoreCase;
            return (m.RobotNo?.Contains(q, cmp) ?? false)
                || (m.RobotName?.Contains(q, cmp) ?? false)
                || (m.RobotType?.Contains(q, cmp) ?? false)
                || (m.IpAddress?.Contains(q, cmp) ?? false);
        }
        // 4) Filter：任一欄位包含關鍵字就顯示
        private bool FilterDevice(object obj)
        {
            if (obj is not DeviceInfo d) return false;

            var q = (DeviceSearchText ?? string.Empty).Trim();
            if (q.Length == 0) return true;

            var cmp = StringComparison.OrdinalIgnoreCase;
            return (d.DeviceNo?.Contains(q, cmp) ?? false)
                || (d.DeviceName?.Contains(q, cmp) ?? false)
                || (d.IpAddress?.Contains(q, cmp) ?? false)
                || d.Port.ToString().Contains(q, cmp);
        }



        public class MachineInfo
        {
            public string MachineId { get; set; } = "";
            public string MachineNo { get; set; } = "";
            public string MachineName { get; set; } = "";
            public string MachineType { get; set; } = "";
            public string IpAddress { get; set; } = "";
            public string Port { get; set; } = ""; // reverted to string
            public string Owner { get; set; } = "";
        }
        public class RobotInfo
        {
            public string RobotId { get; set; } = "";
            public string RobotNo { get; set; } = "";
            public string RobotName { get; set; } = "";
            public string RobotType { get; set; } = "";
            public string IpAddress { get; set; } = "";
            public string Owner { get; set; } = "";
        }
        public class DeviceInfo
        {
            public string DeviceId { get; set; } = "";
            public string DeviceNo { get; set; } = "";
            public string DeviceName { get; set; } = "";
            public string IpAddress { get; set; } = "";
            public string Port { get; set; } = ""; // reverted to string
            public string Owner { get; set; } = "";
        }
        public class WorkerInfo
        {
            public string Id { get; set; } = "";
            public string WorkerNumber { get; set; } = "";
            public string WorkerName { get; set; } = "";
            public string Psssword { get; set; } = "";
        }
        // === 專家模式相關屬性 ===
        [ObservableProperty] private bool showPasswordPrompt; // 是否顯示輸入密碼面板
        [ObservableProperty] private bool isAdvancedEnabled;  // 進階設定 Tab 是否啟用

        partial void OnSelectedPermissionChanged(string value)
        {
            // 選擇權限後觸發：若選擇專家但尚未啟用，顯示密碼輸入面板
            if (value == "專家")
            {
                ShowPasswordPrompt = !IsAdvancedEnabled; // 尚未解鎖才顯示輸入區
            }
            else
            {
                // 切換回工作人員 -> 關閉進階與密碼面板
                IsAdvancedEnabled = false;
                ShowPasswordPrompt = false;
            }
        }
        [ObservableProperty] private string password = "";
        [RelayCommand]
        private void PermissionClick()
        {
            if (SelectedPermission != "專家")
            {
                StatusMessage = "⚠️ 請選擇專家模式再輸入密碼";
                _windowService.ShowMessage("請先選擇專家模式");
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                StatusMessage = "❌ 密碼不可為空";
                _windowService.ShowMessage("請輸入密碼");
                return;
            }

            if (Password == "1758")
            {
                IsAdvancedEnabled = true;          // 啟用進階設定頁籤
                ShowPasswordPrompt = false;        // 隱藏密碼輸入面板
                StatusMessage = "✅ 已開啟專家模式";
                _windowService.ShowMessage("已開啟專家模式");
            }
            else
            {
                IsAdvancedEnabled = false;
                StatusMessage = "❌ 密碼錯誤";
                _windowService.ShowMessage("密碼錯誤，請再試一次");
            }
        }



        #region 保養
        [RelayCommand]
        private void ForceLubrication()
        {
            if (!_authorizationService.RequireLoginAndWriteOperation(28))
                return;
            //待增加
        }


        #endregion
    }
}
