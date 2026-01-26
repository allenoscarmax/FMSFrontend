using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;
using FMSFrontend.Models;
using FMSFrontend.Services;
using IniFile;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq; // 需要
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace FMSFrontend.ViewModels.Factory
{
    public partial class FactoryLayoutViewModel : INotifyPropertyChanged
    {
        // == Service ===
        private readonly IHttpService _httpService;
        private readonly IMachinesService _machinesService;
        private readonly IStorageService _storageService;
        private readonly IRobotService _robotService;
        // === Singleton ===
        public RobotStore RobotStore { get; }
        public Robot Robot => RobotStore.Robot;
        public ObservableCollection<MachineNode> Machines { get; } = new();
        private static string Pack(string rel) => $"/FMSFrontend;component/{rel}";

        private bool isEditMode;
        public bool IsEditMode
        {
            get => isEditMode;
            set { isEditMode = value; OnPropertyChanged(); }
        }


        /// <summary>
        /// // 建議集中定義 Id
        private const string RobotId = "ROBOT";
        private const string TrackId = "Track";
        string robotAtId = "CNC"; // 預設在 CNC（可從外部更新）
        public string RobotAtId
        {
            get => robotAtId;
            set { robotAtId = value; OnPropertyChanged(); UpdateHighlight(); }
        }
        private MachineNode? Find(string id) =>
    Machines.FirstOrDefault(m => string.Equals(m.Id, id, StringComparison.OrdinalIgnoreCase));


        public void SetRobotAt(string machineId) => RobotAtId = machineId;

        //void UpdateHighlight()
        //{
        //    MachineNode? target = null;

        //    foreach (var m in Machines)
        //    {
        //        var active = string.Equals(m.Id, RobotAtId, StringComparison.OrdinalIgnoreCase);
        //        m.IsActive = active;
        //        if (active) target = m;
        //    }

        //    var robot = Find(RobotId);
        //    if (robot != null && target != null && !ReferenceEquals(robot, target))
        //    {
        //        AlignRobotToTargetX(robot, target);
        //    }
        //}
        // ✅ 只計算高亮與目標座標 → 觸發事件給 View 做動畫
        // 🔔 提供給 View 訂閱用：請在 View 裡接到後做動畫
        public event Action<MachineNode, double, double>? RobotMoveRequested;
        private void UpdateHighlight()
        {
            MachineNode? target = null;
            if (RobotAtId == null || RobotAtId == "") return;
            foreach (var m in Machines)
            {
                // var active = string.Equals(m.Id, RobotAtId, StringComparison.OrdinalIgnoreCase); 
                var active = m.Id.IndexOf(RobotAtId, StringComparison.OrdinalIgnoreCase) >= 0;
                m.IsActive = active;
                if (active) target = m;
            }

            var robot = Find(RobotId);
            if (robot == null || target == null || ReferenceEquals(robot, target))
                return;

            // 計算目標 X/Y（置中對齊，Y 放在軌道上方一點）
            var targetWidth = target.Width > 0 ? target.Width : 150;
            var robotWidth = robot.Width > 0 ? robot.Width : 130;

            var targetX = target.X + (targetWidth - robotWidth) / 2.0;

            var targetY = robot.Y; // 預設不動 Y
            var track = Find(TrackId);
            if (track != null)
            {
                if (robot.Height > 0)
                    targetY = track.Y - 160;
                if (track.Width + track.X < targetX)
                    targetX = track.Width + track.X - 85;
                if (targetX < track.X)
                    targetX = track.X - 20;
            }
            // 🔔 通知 View（UserControl）去做動畫
            RobotMoveRequested?.Invoke(robot, targetX, targetY);
        }

        private void AlignRobotToTargetX(MachineNode robot, MachineNode target)
        {
            // 以兩者的中心線對齊 X
            var targetWidth = target.Width > 0 ? target.Width : 150;
            var robotWidth = robot.Width > 0 ? robot.Width : 130;

            robot.X = target.X + (targetWidth - robotWidth) / 2.0;

            // 如需同時調整 Y：把手臂放在軌道上方（可留著或拿掉）
            var track = Find(TrackId);
            if (track != null && robot.Height > 0)
            {
                // 輕微疊在軌道上，視覺比較自然；可把 4 改成你想要的縫隙
                robot.Y = track.Y - robot.Height + 1;
            }
        }
        /// </summary>


        public FactoryLayoutViewModel(IHttpService httpService, 
            IMachinesService machinesService,
            IStorageService storageService,
            IRobotService robotService,
            RobotStore robotStore)
        {
            _httpService = httpService;
            _machinesService = machinesService;
            _storageService = storageService;
            _robotService = robotService;
            RobotStore = robotStore;
            UpdateHighlight(); // 初始化一次
            Robot.PropertyChanged += RobotOnPropertyChanged;
        }
        private void RobotOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Robot.CurrentLocation))
            {
                RefreshFromStore();
            }
        }
        void RefreshFromStore()
        {
            //var pos = Robot.CurrentLocation;
            // 找出對應 Machine（Id 要是 EDM1 / EDM2 / EDM3 / EW1 / ASE1）
            //var target = Machines.FirstOrDefault(m => string.Equals(m.Id, pos, StringComparison.OrdinalIgnoreCase));
            //if (string.IsNullOrWhiteSpace(pos) )return;
            // 呼叫移動函式
            SetRobotAt(Robot.CurrentLocation);
        }
        public async Task SaveLayoutAsync(string path)
        {
            var json = JsonSerializer.Serialize(Machines, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(path, json);
        }

        private static readonly Random _rnd = new Random();
        public async Task LoadLayoutAsync(string path)
        {

            if (!File.Exists(path)) return;
            var json = await File.ReadAllTextAsync(path);
            var nodes = JsonSerializer.Deserialize<ObservableCollection<MachineNode>>(json);
            if (nodes == null) return;
            Machines.Clear();
            foreach (var n in nodes) Machines.Add(n);
            /*
            //測試
            Machines.Clear();
            Machines.Add(new MachineNode { Id = "EDM1", DisplayName = "EDM1", X = 80, Y = 60, IconPath = Pack("Image/MachineIcons/EDM.png") });
            Machines.Add(new MachineNode { Id = "EDM2", DisplayName = "EDM2", X = 300, Y = 60, IconPath = Pack("Image/MachineIcons/EDM.png") });
            Machines.Add(new MachineNode { Id = "EDM3", DisplayName = "EDM3", X = 520, Y = 60, IconPath = Pack("Image/MachineIcons/EDM.png") });

            Machines.Add(new MachineNode { Id = "ROBOT", DisplayName = "Robot", X = 80, Y = 250, Width = 130, Height = 130, IconPath = Pack("Image/MachineIcons/Robot.png") });

            Machines.Add(new MachineNode { Id = "Track", DisplayName = "", X = 80, Y = 400, Width = 850, Height = 80, IconPath = Pack("Image/MachineIcons/long-track.png") });
            Machines.Add(new MachineNode { Id = "FMS", DisplayName = "FMS", X = 80, Y = 500, IconPath = Pack("Image/MachineIcons/FMS.png") });
            Machines.Add(new MachineNode { Id = "ES1", DisplayName = "ES1", X = 300, Y = 500, Width = 150, Height = 150, IconPath = Pack("Image/MachineIcons/Magzine.png") });
            */
        }
        public async Task LayoutInit()
        {
            Machines.Clear();
            //978*822
            int WinWidth = 968; //視窗寬度
            int X_str = 10;    //起始位置X
            int MarginW = 10;  //圖片距離

            //第一排 機台
            int MachH = 140;   //每台機台圖片高度
            int MachY = 30;    //每台機台圖片高度
            //第二排 機械手臂與軌道       
            int RobotW = 110;   //機械手臂圖片寬度
            int RobotH = 130;   //機械手臂圖片高度
            int TrackH = 40;    //軌道高度
            int TrackY = 430;   //軌道位置Y
            //第三排 倉儲 與工作站(與倉儲同)
            int StorageH = 140; //每個倉儲圖片高度
            int StorageY = 500;   //每個倉儲圖片高度

            //讀取機台資料
            List<MachinesDto> dtos = await _machinesService.GetAllMachinesAsync() ?? [];
            for (int i = 0; i < dtos.Count; i++)
            {
                var dto = dtos[i];
                Machines.Add(new MachineNode
                {
                    Id = dto.machineName,
                    DisplayName = dto.machineName,
                    X = X_str + WinWidth / dtos.Count * i,
                    Y = MachY,
                    Width = WinWidth / dtos.Count - MarginW,
                    Height = MachH,
                    IconPath = Pack($"Image/MachineIcons/EDM.png")
                });
            }
            int StorageCnt = 0; //計算實際有幾個倉儲位置

            //讀取工作站資料
            INIFile ini = new INIFile(AppDomain.CurrentDomain.BaseDirectory + "Basesitting.ini");
            int stationCount = 0;
            try
            {
                stationCount = Convert.ToInt16(ini.Read("Prarm", "StationCount"));
                stationCount = 0;
                if (stationCount != 0)
                {
                    Machines.Add(new MachineNode
                    {
                        Id = "工作站",
                        DisplayName = "工作站",
                        //  X = X_str,
                        Y = StorageY,
                        //   Width = StorageW,
                        Height = StorageH,
                        IconPath = Pack("Image/MachineIcons/FMS.png")
                    });
                    StorageCnt++;
                }
            }
            catch { }

            //讀取倉儲資料
            List<StorageDto> storageDtos = await _storageService.GetAllStorageAsync() ?? [];
            // 將 storageName 為 E 或 W，且 storageNumber 為數字 的資料，依 number 分組
            var storages = storageDtos
                .Where(s => !string.IsNullOrWhiteSpace(s.storageName) && !string.IsNullOrWhiteSpace(s.storageNumber))
                .Where(s => (string.Equals(s.storageName, "E", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(s.storageName, "W", StringComparison.OrdinalIgnoreCase))
                            && int.TryParse(s.storageNumber, out _))
                .GroupBy(s => int.Parse(s.storageNumber))
                .OrderBy(g => g.Key);
            foreach (var g in storages)
            {
                int number = g.Key;
                bool hasE = g.Any(x => string.Equals(x.storageName, "E", StringComparison.OrdinalIgnoreCase));
                bool hasW = g.Any(x => string.Equals(x.storageName, "W", StringComparison.OrdinalIgnoreCase));
                // 依需求：若同一號碼同時有 E 與 W，僅保留一筆，Id/DisplayName = "E{n}/W{n}"；否則分別為 "E{n}" 或 "W{n}"
                string id;
                if (hasE && hasW)
                    id = $"E{number}/W{number}";
                else if (hasE)
                    id = $"E{number}";
                else if (hasW)
                    id = $"W{number}";
                else
                    continue;
                // 避免重複加入相同 Id
                if (Machines.Any(m => string.Equals(m.Id, id, StringComparison.OrdinalIgnoreCase))) continue;
                Machines.Add(new MachineNode
                {
                    Id = id,
                    DisplayName = id,
                    //  X = X_str + (StorageW + MarginW) * (number - 1 + stationCount),
                    Y = StorageY,
                    //    Width = StorageW,
                    Height = StorageH,
                    IconPath = Pack("Image/MachineIcons/Magzine.png")
                });
                StorageCnt++;
            }
            //計算寬度
            for (int i = 0; i < StorageCnt; i++)
            {
                Machines[Machines.Count - StorageCnt + i].X = X_str + WinWidth / StorageCnt * i;
                Machines[Machines.Count - StorageCnt + i].Width = WinWidth / StorageCnt - MarginW;
                //  Machines[Machines.Count - StorageCnt + i].Height = WinWidth / StorageCnt - MarginW;
            }

            //讀取機械手臂資料
            List<RobotDto> robotDtos = await _robotService.DB_GetAllRobotsAsync() ?? [];
            for (int i = 0; i < robotDtos.Count; i++)
            {
                var dto = robotDtos[i];
                Machines.Add(new MachineNode
                {
                    Id = "ROBOT",
                    DisplayName = "ROBOT",
                    X = X_str,
                    Y = TrackY - 160,
                    Width = RobotW,
                    Height = RobotH,
                    IconPath = Pack($"Image/MachineIcons/Robot.png")
                });
                Machines.Add(new MachineNode
                {
                    Id = "Track",
                    DisplayName = "",
                    X = X_str,
                    Y = TrackY,
                    Width = WinWidth - 250,
                    Height = TrackH,
                    IconPath = Pack($"Image/MachineIcons/long-track.png")
                });
            }
            foreach (var a in Machines)
            {
                if(a.Id == "Track") a.Stretch = Stretch.Uniform;
                else a.Stretch = Stretch.Uniform;
            }
        }
        [RelayCommand]
        private async Task onMove()
        {
            await Moverobot();
        }

        private async Task Moverobot()
        {
            await Task.Delay(1); // 保持 async 簽名

            // 所有可去的機台 Id（不包含手臂/軌道）
            var candidates = Machines
                .Where(m => m.Id != "ROBOT" && m.Id != "Track")
                .Select(m => m.Id)
                .ToList();

            if (!candidates.Any())
                return;

            // 隨機選一個
            var nextId = candidates[_rnd.Next(candidates.Count)];

            // 移動手臂
            SetRobotAt(nextId);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class MachineNode : INotifyPropertyChanged
    {
        public string Id { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public double Width { get; set; } = 120;
        public double Height { get; set; } = 120;
        public string? IconPath { get; set; }   // 直接用字串即可
        public Stretch Stretch { get; set; }

        private double x;
        public double X { get => x; set { x = value; OnPropertyChanged(); } }

        private double y;
        public double Y { get => y; set { y = value; OnPropertyChanged(); } }

        bool isActive;
        public bool IsActive { get => isActive; set { isActive = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
