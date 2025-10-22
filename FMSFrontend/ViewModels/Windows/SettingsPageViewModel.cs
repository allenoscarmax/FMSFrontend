using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Extensions;   // ← PeriodWindow / PeriodWindowArgs / PeriodSelectionResult / ScheduleMode
using FMSFrontend.Interfaces;
using FMSFrontend.Services;
using IniFile;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace FMSFrontend.ViewModels.Windows
{
    public partial class SettingsPageViewModel : ObservableObject
    {
        // ===== 顯示用（你的 XAML 綁在這個上面）=====
        [ObservableProperty] private string periodDisplay = "不設定";

        // 新增：綁定系統 IP 的輸入欄位
        [ObservableProperty] private string serverIp = string.Empty;

        // ===== 目前的設定（當作下一次開窗的初始值）=====
        [ObservableProperty] private ScheduleMode selectedMode = ScheduleMode.None;
        public ObservableCollection<int> Weekly { get; } = new();   // 1~7、可負數
        public ObservableCollection<int> Monthly { get; } = new();   // 1~31、0=last、可負數
        [ObservableProperty] private int hour12 = 9;   // 1~12
        [ObservableProperty] private int minute = 41;  // 0~59
        [ObservableProperty] private bool isPm = false;

        // ===== 開窗指令 ── 綁到你的 Button =====
        public IRelayCommand OpenSetPeriodDialogCommand { get; }

        private readonly IWindowService _windowService;
        private readonly IHttpService _httpService;

        public SettingsPageViewModel(IWindowService windowService, IHttpService httpService)
        {
            _windowService = windowService;
            _httpService = httpService;

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


            MachineList = new ObservableCollection<MachineInfo>{
    new MachineInfo { MachineId="EDM1", MachineName="EDM1", MachineType="EDM1", IpAddress="192.168.21.232", Port=13101, AssetNo="-", Owner="OscarMax" },
    new MachineInfo { MachineId="EDM2", MachineName="EDM2", MachineType="EDM2", IpAddress="192.168.21.233", Port=13102, AssetNo="-", Owner="OscarMax" },
    new MachineInfo { MachineId="EDM3", MachineName="EDM3", MachineType="EDM3", IpAddress="192.168.21.234", Port=13103, AssetNo="-", Owner="OscarMax" },
    new MachineInfo { MachineId="EDM4", MachineName="EDM4", MachineType="EDM4", IpAddress="192.168.21.235", Port=13104, AssetNo="-", Owner="OscarMax" },
    new MachineInfo { MachineId="EDM5", MachineName="EDM5", MachineType="EDM5", IpAddress="192.168.21.236", Port=13105, AssetNo="-", Owner="OscarMax" },
};
            _machinesView = CollectionViewSource.GetDefaultView(MachineList);
            _machinesView.Filter = FilterMachine;        // 設定一次即可

            RobotList = new ObservableCollection<RobotInfo>{
    new RobotInfo { RobotId="Robot1", RobotName="Robot1", RobotType="Fanuc", IpAddress="192.168.21.6", Owner="OscarMax" },
};
            _robotsView = CollectionViewSource.GetDefaultView(RobotList);
            _robotsView.Filter = FilterRobot;        // 設定一次即可

            // Device 假資料（依你的截圖）
            DeviceList = new ObservableCollection<DeviceInfo>
{
    new() { DeviceId="Delta_IO",      DeviceName="Delta_IO",      IpAddress="192.168.21.239", Port=10001, AssetNo="-", Owner="OscarMax" },
    new() { DeviceId="Balluff_RFID",  DeviceName="Balluff_RFID",  IpAddress="192.168.21.236", Port=10001, AssetNo="-", Owner="OscarMax" },
    new() { DeviceId="ESL",           DeviceName="ESL",           IpAddress="192.168.21.238", Port=10001, AssetNo="-", Owner="OscarMax" },
};

            // 建立 View + Filter
            _devicesView = CollectionViewSource.GetDefaultView(DeviceList);
            _devicesView.Filter = FilterDevice;
        }
        // 這就是缺少的屬性，用來綁定 Tab 切換
        [ObservableProperty]
        private int selectedTabIndexParameter;
        [ObservableProperty]
        private int selectedSubTabIndexParameter;
        // === 設定屬性 ===
        [ObservableProperty]
        private ObservableCollection<string> availableLanguages;

        [ObservableProperty]
        private string selectedLanguage;

        [ObservableProperty]
        private ObservableCollection<string> availableThemes;

        [ObservableProperty]
        private string selectedTheme;

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
        private string selectedPermission;

        //MachineList
        public ObservableCollection<MachineInfo> MachineList { get; set; }
        private readonly ICollectionView _machinesView;
        private string _searchText;
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
        //RobotList
        public ObservableCollection<RobotInfo> RobotList { get; set; }
        private readonly ICollectionView _robotsView;
        private string _robotSearchText;
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
        }// ====== Device 區 ======

        // 1) 清單 + 視圖
        public ObservableCollection<DeviceInfo> DeviceList { get; set; }
        private readonly ICollectionView _devicesView;

        // 2) 選取項目（可選）
        private DeviceInfo _selectedDevice;
        public DeviceInfo SelectedDevice
        {
            get => _selectedDevice;
            set => SetProperty(ref _selectedDevice, value);
        }

        // 3) 搜尋文字（即時過濾）
        private string _deviceSearchText;
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
            var win = new PeriodWindow
            {
                Owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
            };

            var vm = (PeriodWindowViewModel)win.DataContext;
            vm.ApplyInitial(SelectedMode, Weekly.ToArray(), Monthly.ToArray(), Hour12, Minute, IsPm, "潤滑週期");

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

                Hour12 = vm.Hour;
                Minute = vm.Minute;
                IsPm = vm.IsPm;

                PeriodDisplay = BuildPeriodDisplay(SelectedMode, Weekly.ToArray(), Monthly.ToArray(), Hour12, Minute, IsPm);
            }
        }

        private static string BuildPeriodDisplay(ScheduleMode mode, int[] weekly, int[] monthly, int hour12, int minute, bool isPm)
        {
            if (mode == ScheduleMode.None) return "不設定";
            string time = $"{hour12:00}:{minute:00} {(isPm ? "PM" : "AM")}";

            if (mode == ScheduleMode.Weekly)
            {
                string[] w = { "週一", "週二", "週三", "週四", "週五", "週六", "週日" };
                string L(int v) => v > 0 ? w[v - 1] : "-" + w[-v - 1];
                return $"每週 {string.Join(" ", weekly.Select(L))} {time} 進行潤滑";
            }
            else
            {
                string L(int v) => v == 0 ? "last" : v.ToString();
                return $"每月 {string.Join(" ", monthly.Select(L))} {time} 進行潤滑";
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

        // === 機器人維護完成 ===
        [RelayCommand]
        private void CompleteRobotMaintenance()
        {
            // TODO: 加入維護完成標記邏輯
            StatusMessage = "🤖 機器人維護已標記為完成";
            _windowService.ShowMessage("維護狀態已更新");
        }
        // === 設定IP ===
        [RelayCommand]
        private void SaveIP()
        {
            var ip = (ServerIp ?? string.Empty).Trim();
            if (!IPAddress.TryParse(ip, out _))
            {
                StatusMessage = "❌ IP 位址格式不正確";
                _windowService.ShowMessage("IP 位址格式不正確，請輸入有效的 IPv4，例如：192.168.1.100");
                return;
            }
            try
            {
                _httpService.UpdateServerIp(ip);
                StatusMessage = "🤖 設定IP OK";
                _windowService.ShowMessage($"已設定 IP：{ip}");
                INIFile ini = new INIFile(AppDomain.CurrentDomain.BaseDirectory + "\\Basesitting.ini");
                ini.Write("Prarm", "IP", ip);
            }
            catch (Exception ex)
            {
                StatusMessage = "❌ 設定 IP 失敗";
                _windowService.ShowMessage($"設定 IP 失敗：{ex.Message}");
            }
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
                || m.Port.ToString().Contains(q, cmp)
                || (m.AssetNo?.Contains(q, cmp) ?? false)
                || (m.Owner?.Contains(q, cmp) ?? false);
        }
        private bool FilterRobot(object obj)
        {
            if (obj is not RobotInfo m) return false;

            var q = (RobotSearchText ?? string.Empty).Trim();
            if (q.Length == 0) return true;

            var cmp = StringComparison.OrdinalIgnoreCase;
            return (m.RobotId?.Contains(q, cmp) ?? false)
                || (m.RobotName?.Contains(q, cmp) ?? false)
                || (m.RobotType?.Contains(q, cmp) ?? false)
                || (m.IpAddress?.Contains(q, cmp) ?? false)
                || (m.Owner?.Contains(q, cmp) ?? false);
        }
        // 4) Filter：任一欄位包含關鍵字就顯示
        private bool FilterDevice(object obj)
        {
            if (obj is not DeviceInfo d) return false;

            var q = (DeviceSearchText ?? string.Empty).Trim();
            if (q.Length == 0) return true;

            var cmp = StringComparison.OrdinalIgnoreCase;
            return (d.DeviceId?.Contains(q, cmp) ?? false)
                || (d.DeviceName?.Contains(q, cmp) ?? false)
                || (d.IpAddress?.Contains(q, cmp) ?? false)
                || d.Port.ToString().Contains(q, cmp)
                || (d.AssetNo?.Contains(q, cmp) ?? false)
                || (d.Owner?.Contains(q, cmp) ?? false);
        }

       

        public class MachineInfo
        {
            public string MachineId { get; set; }
            public string MachineName { get; set; }
            public string MachineType { get; set; }
            public string IpAddress { get; set; }
            public int Port { get; set; }
            public string AssetNo { get; set; }
            public string Owner { get; set; }
        }
        public class RobotInfo
        {
            public string RobotId { get; set; }
            public string RobotName { get; set; }
            public string RobotType { get; set; }
            public string IpAddress { get; set; }
            public string Owner { get; set; }
        }
        public class DeviceInfo
        {
            public string DeviceId { get; set; }
            public string DeviceName { get; set; }
            public string IpAddress { get; set; }
            public int Port { get; set; }
            public string AssetNo { get; set; }
            public string Owner { get; set; }
        }

    }
}
