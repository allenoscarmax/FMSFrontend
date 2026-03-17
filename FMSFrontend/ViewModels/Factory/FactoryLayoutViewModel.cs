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
    public partial class FactoryLayoutViewModel : INotifyPropertyChanged, IDisposable
    {
        // == Service ===
        private readonly IHttpService _httpService;
        private readonly IMachinesService _machinesService;
        private readonly IStorageService _storageService;
        private readonly IRobotService _robotService;
        private readonly AMRLiveUpdater _amrLiveUpdater;
        // === Singleton ===
        public RobotStore RobotStore { get; }
        public AMRStore AMRStore { get; }
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
        private const string Robot2Id = "ROBOT2";
        string robotAtId = "CNC"; // 預設在 CNC（可從外部更新）
        private string robot2AtId = "Target1";
        public string RobotAtId
        {
            get => robotAtId;
            set { robotAtId = value; OnPropertyChanged(); UpdateHighlight(); }
        }
        public string Robot2AtId
        {
            get => robot2AtId;
            set { robot2AtId = value; OnPropertyChanged(); UpdateRobot2Highlight(); }
        }
        private MachineNode? Find(string id) =>
    Machines.FirstOrDefault(m => string.Equals(m.Id, id, StringComparison.OrdinalIgnoreCase));


        public void SetRobotAt(string machineId) => RobotAtId = machineId;
        public void SetRobot2At(string targetId) => Robot2AtId = targetId;

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
        public event Action<MachineNode, double, double>? Robot2MoveRequested;
        private void UpdateHighlight()
        {
            MachineNode? target = null;
            if (RobotAtId == null || RobotAtId == "") return;
            foreach (var m in Machines)
            {
                if (m.Id.StartsWith("Target", StringComparison.OrdinalIgnoreCase))
                    continue;
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
                    targetY = track.Y - 130;
                if (targetX > track.Width + track.X - 85)
                    targetX = track.Width + track.X - 85;
                if (targetX < track.X - 20)
                    targetX = track.X - 20;
            }
            // 🔔 通知 View（UserControl）去做動畫
            RobotMoveRequested?.Invoke(robot, targetX, targetY);
        }

        private void UpdateRobot2Highlight()
        {
            if (string.IsNullOrWhiteSpace(Robot2AtId)) return;

            foreach (var m in Machines.Where(x => x.Id.StartsWith("Target", StringComparison.OrdinalIgnoreCase)))
            {
                m.IsActive = string.Equals(m.Id, Robot2AtId, StringComparison.OrdinalIgnoreCase);
            }

            var target = Find(Robot2AtId);
            var robot2 = Find(Robot2Id);
            if (robot2 == null || target == null || ReferenceEquals(robot2, target))
                return;

            var targetWidth = target.Width > 0 ? target.Width : 130;
            var robotWidth = robot2.Width > 0 ? robot2.Width : 130;
            var targetX = target.X + (targetWidth - robotWidth) / 2.0+15;
            var targetY = target.Y+40;

            Robot2MoveRequested?.Invoke(robot2, targetX, targetY);
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
            RobotStore robotStore,
            AMRStore amrStore,
            AMRLiveUpdater amrLiveUpdater)
        {
            _httpService = httpService;
            _machinesService = machinesService;
            _storageService = storageService;
            _robotService = robotService;
            _amrLiveUpdater = amrLiveUpdater;
            RobotStore = robotStore;
            AMRStore = amrStore;
            //UpdateHighlight(); // 初始化一次
            Robot.PropertyChanged += RobotOnPropertyChanged;
            AMRStore.Amr.PropertyChanged += AmrOnPropertyChanged;
            _amrLiveUpdater.Start();
        }

        private void AmrOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AMRModel.Location))
            {
                ApplyAmrLocation(AMRStore.Amr.Location);
            }
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

        public void ApplyAmrLocation(string? location)
        {
            if (string.IsNullOrWhiteSpace(location))
                return;

            if (location.Contains("machine", StringComparison.OrdinalIgnoreCase))
            {
                SetRobot2At("Target1");
            }
            else if (location.Contains("item", StringComparison.OrdinalIgnoreCase))
            {
                SetRobot2At("Target2");
            }
        }

        public async Task SaveLayoutAsync(string path)
        {
            var json = JsonSerializer.Serialize(Machines, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(path, json);
        }

        private static readonly Random _rnd = new Random();
        private int _robot2TargetIndex = 0;
        public async Task LoadLayoutAsync(string path)
        {

            if (!File.Exists(path)) return;
            var json = await File.ReadAllTextAsync(path);
            var nodes = JsonSerializer.Deserialize<ObservableCollection<MachineNode>>(json);
            if (nodes == null) return;
            Machines.Clear();
            foreach (var n in nodes) Machines.Add(n);
            /*
            //測試 佑義Layout
            Machines.Clear();
            Machines.Add(new MachineNode { Id = "EDM1", DisplayName = "EDM1", X = 80, Y = 60, IconPath = Pack("Image/MachineIcons/EDM.png") });
            Machines.Add(new MachineNode { Id = "EDM2", DisplayName = "EDM2", X = 300, Y = 60, IconPath = Pack("Image/MachineIcons/EDM.png") });
            Machines.Add(new MachineNode { Id = "EDM3", DisplayName = "EDM3", X = 520, Y = 60, IconPath = Pack("Image/MachineIcons/EDM.png") });

            Machines.Add(new MachineNode { Id = "ROBOT", DisplayName = "Robot", X = 80, Y = 250, Width = 130, Height = 130, IconPath = Pack("Image/MachineIcons/Robot.png") });

            Machines.Add(new MachineNode { Id = "Track", DisplayName = "", X = 80, Y = 400, Width = 850, Height = 80, IconPath = Pack("Image/MachineIcons/long-track.png") });
            Machines.Add(new MachineNode { Id = "FMS", DisplayName = "FMS", X = 80, Y = 500, IconPath = Pack("Image/MachineIcons/FMS.png") });
            Machines.Add(new MachineNode { Id = "ES1", DisplayName = "ES1", X = 300, Y = 500, Width = 150, Height = 150, IconPath = Pack("Image/MachineIcons/Magzine.png") });
            */
            
            //TMTS Layout
            Machines.Clear();
            Machines.Add(new MachineNode { Id = "CMM", DisplayName = "MiSTAR 555",                 X = 750, Y = 5,  Width = 130, Height = 130, IconPath = Pack("Image/MachineIcons/CMM.png") });
            Machines.Add(new MachineNode { Id = "FanucCNC1", DisplayName = "JHV-550",     X = 500, Y = 5,  Width = 140, Height = 140, IconPath = Pack("Image/MachineIcons/CNC.png") });
            Machines.Add(new MachineNode { Id = "SiemensCNC1", DisplayName = "UH-500", X = 250, Y = 5,  Width = 150, Height = 150, IconPath = Pack("Image/MachineIcons/UH500.png") });
            Machines.Add(new MachineNode { Id = "EDM1", DisplayName = "EX-60",               X = 20,  Y = 190, Width = 150, Height = 150, IconPath = Pack("Image/MachineIcons/EDM.png") });
            Machines.Add(new MachineNode { Id = "ROBOT", DisplayName = "Robot1",            X = 170, Y = 200, Width = 120, Height = 120, IconPath = Pack("Image/MachineIcons/Robot.png") });
            Machines.Add(new MachineNode { Id = "Track", DisplayName = "",                  X = 200, Y = 320, Width = 700, Height = 40,  IconPath = Pack("Image/MachineIcons/long-track.png") });
            Machines.Add(new MachineNode { Id = "E1", DisplayName = "E1",                   X = 375, Y = 370, Width = 130, Height = 120, IconPath = Pack("Image/MachineIcons/Magzine.png") });
            Machines.Add(new MachineNode { Id = "W1", DisplayName = "W1",                   X = 625, Y = 370, Width = 130, Height = 120, IconPath = Pack("Image/MachineIcons/Magzine.png") });
            Machines.Add(new MachineNode { Id = "EDM2", DisplayName = "ESD-435",                X = 20,  Y = 540, Width = 150, Height = 150, IconPath = Pack("Image/MachineIcons/ESD.png") });
            Machines.Add(new MachineNode { Id = "Target1", DisplayName = "Target1",         X = 195, Y = 530, Width = 50, Height = 200, IconPath = Pack("Image/MachineIcons/Target.png") });
            Machines.Add(new MachineNode { Id = "Target2", DisplayName = "Target2",         X = 650, Y = 530, Width = 50, Height = 200, IconPath = Pack("Image/MachineIcons/Target.png") });
            Machines.Add(new MachineNode { Id = "ROBOT2", DisplayName = "Robot2",           X = 750, Y = 570, Width = 120, Height = 120, IconPath = Pack("Image/MachineIcons/RobotOnAMR.png") });
            SetRobotAt(Robot.CurrentLocation);
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
            double Width = 150;
            //讀取機台資料
            List<MachinesDto> dtos = await _machinesService.GetAllMachinesAsync() ?? [];
            for (int i = 0; i < dtos.Count; i++)
            {
                var dto = dtos[i];
                Width = WinWidth / dtos.Count - MarginW;
                Width = Width > 150 ? 150 : Width;
                Machines.Add(new MachineNode
                {
                    Id = dto.machineName,
                    DisplayName = dto.machineName,
                    X = X_str + WinWidth / dtos.Count * (i + 0.5) - Width * 0.5,
                    Y = MachY,
                    Width = Width,
                    Height = MachH,
                    IconPath = Pack($"Image/MachineIcons/EDM.png")
                });
            }
            int StorageCnt = 0; //計算實際有幾個倉儲位置

            //讀取工作站資料
            INIFile ini = new INIFile(AppDomain.CurrentDomain.BaseDirectory + "Basesitting.ini");
            int stationCount = 0; //工作站數量 鋐興:0 佑義:1
            try
            {
                //stationCount = Convert.ToInt16(ini.Read("Prarm", "StationCount"));
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
            Width = WinWidth / StorageCnt - MarginW;
            Width = Width > 150 ? 150 : Width;
            for (int i = 0; i < StorageCnt; i++)
            {
                Machines[Machines.Count - StorageCnt + i].X = X_str + WinWidth / StorageCnt * (i + 0.5) - Width * 0.5;
                Machines[Machines.Count - StorageCnt + i].Width = Width;
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
                    X = X_str+ WinWidth*0.5 - RobotW*0.5,
                    Y = TrackY - 160,
                    Width = RobotW,
                    Height = RobotH,
                    IconPath = Pack($"Image/MachineIcons/Robot.png")
                });
                Machines.Add(new MachineNode
                {
                    Id = "Track",
                    DisplayName = "",
                    X = 150,
                    Y = TrackY,
                    Width = WinWidth - 300,
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

        [RelayCommand]
        private async Task onMoveRobot2()
        {
            await MoveRobot2SequentialAsync();
        }

        private async Task Moverobot()
        {
            await Task.Delay(1); // 保持 async 簽名

            // 所有可去的機台 Id（不包含手臂/軌道）
            var candidates = Machines
                .Where(m => !string.Equals(m.Id, "ROBOT", StringComparison.OrdinalIgnoreCase)
                         && !string.Equals(m.Id, "TRACK", StringComparison.OrdinalIgnoreCase)
                         && !string.Equals(m.Id, "ROBOT2", StringComparison.OrdinalIgnoreCase)
                         && !string.Equals(m.Id, "ESD", StringComparison.OrdinalIgnoreCase)
                         && !string.Equals(m.DisplayName, "ESD", StringComparison.OrdinalIgnoreCase)
                         && !m.Id.StartsWith("Target", StringComparison.OrdinalIgnoreCase))
                .Select(m => m.Id)
                .ToList();

            if (!candidates.Any())
                return;

            // 隨機選一個
            var nextId = candidates[_rnd.Next(candidates.Count)];

            // 移動手臂
            SetRobotAt(nextId);
        }

        private async Task MoveRobot2SequentialAsync()
        {
            await Task.Delay(1);

            var orderedTargets = Machines
                .Where(m => m.Id.StartsWith("Target", StringComparison.OrdinalIgnoreCase))
                .OrderBy(m =>
                {
                    var suffix = m.Id.Length > 6 ? m.Id[6..] : string.Empty;
                    return int.TryParse(suffix, out var n) ? n : int.MaxValue;
                })
                .Select(m => m.Id)
                .ToList();

            if (!orderedTargets.Any())
                return;

            if (_robot2TargetIndex >= orderedTargets.Count)
                _robot2TargetIndex = 0;

            SetRobot2At(orderedTargets[_robot2TargetIndex]);
            _robot2TargetIndex = (_robot2TargetIndex + 1) % orderedTargets.Count;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public void Dispose()
        {
            Robot.PropertyChanged -= RobotOnPropertyChanged;
            AMRStore.Amr.PropertyChanged -= AmrOnPropertyChanged;
            _amrLiveUpdater.Stop();
        }
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

