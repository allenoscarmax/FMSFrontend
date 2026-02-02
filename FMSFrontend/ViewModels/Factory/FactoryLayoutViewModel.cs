using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq; // 需要
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace FMSFrontend.ViewModels.Factory
{
    public partial class FactoryLayoutViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<MachineNode> Machines { get; } = new();
        private static string Pack(string rel) => $"/FMSFrontend;component/{rel}";

        private bool isEditMode;
        public bool IsEditMode
        {
            get => isEditMode;
            set { isEditMode = value; OnPropertyChanged(); }
        }

        // 可以放在 ViewModel / code-behind 的欄位
        private readonly HashSet<string> _validRobotPositions =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "EW1", "EDM1", "EDM2", "EDM3", "ASE1" };

        private string _currentRobotPosition;  // 記住目前畫面上的位置（選用）


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

        public event Action<MachineNode, double, double>? RobotMoveRequested;
        private void UpdateHighlight()
        {
            MachineNode? target = null;

            foreach (var m in Machines)
            {
                var active = string.Equals(m.Id, RobotAtId, StringComparison.OrdinalIgnoreCase);
                m.IsActive = active;
                if (active) target = m;
            }

            var robot = Find(RobotId);
            if (robot == null || target == null || ReferenceEquals(robot, target))
                return;

            double targetX;

            // ============================
            // ⭐ 特例：EDM1 要排在「軌道的最右側」
            // ============================
            if (string.Equals(target.Id, "EDM1", StringComparison.OrdinalIgnoreCase))
            {
                var track = Find(TrackId);
                if (track != null)
                {
                    // 手臂放軌道最右側（扣掉自己的寬）
                    targetX = track.X + track.Width - robot.Width - 5;  // -5 可微調間距
                }
                else
                {
                    targetX = target.X; // 找不到軌道就退回普通模式
                }
            }
            else
            {
                // ============================
                // ⭐ 一般機台：置中對齊
                // ============================
                var targetWidth = target.Width > 0 ? target.Width : 150;
                var robotWidth = robot.Width > 0 ? robot.Width : 130;

                targetX = target.X + (targetWidth - robotWidth) / 2.0;
            }

            // ============================
            // ⭐ Y 座標：放在軌道上方
            // ============================
            var targetY = robot.Y;
            var trackNode = Find(TrackId);

            if (trackNode != null && robot.Height > 0)
                targetY = trackNode.Y - robot.Height + 1;

            // ⭐ 通知前端動畫
            RobotMoveRequested?.Invoke(robot, targetX, targetY);
        }


         /// </summary>
        //控制區按鈕
        public RobotStore RobotStore { get; }
        public Robot Robot => RobotStore.Robot;
        public FactoryLayoutViewModel(RobotStore store)
        {
            RobotStore = store;
            Robot.PropertyChanged += RobotOnPropertyChanged;

            //// 1. 先把 RobotAtId 同步到畫面
            //SyncRobotPositionFromStore();
            //// 2. 再依照同步後的 RobotAtId 做第一次 highlight 與定位
            //UpdateHighlight();


        }
        private void RobotOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Robot.CurrentLocation))
            {
                SyncRobotPositionFromStore();
            }
        }
        /// <summary>
        /// 根據 Robot.CurrentLocation 更新畫面上的手臂位置
        /// </summary>
        private void SyncRobotPositionFromStore()
        {
            var pos = Robot.CurrentLocation;

            if (string.IsNullOrWhiteSpace(pos))
                return;

            if (!_validRobotPositions.Contains(pos))
                return;

            // 找出對應 Machine（Id 要是 EDM1 / EDM2 / EDM3 / EW1 / ASE1）
            var target = Machines.FirstOrDefault(
                m => string.Equals(m.Id, pos, StringComparison.OrdinalIgnoreCase));

            if (target == null)
                return;

            // 這是你原本的移動函式
            SetRobotAt(target.Id);
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

            SyncRobotPositionFromStore(); // 內部會呼叫 SetRobotAt(...)
            //////測試
            //Machines.Clear();
            //Machines.Add(new MachineNode { Id = "EDM1", DisplayName = "EDM1", X = 780, Y = 230, Width = 150, Height = 200, IconPath = Pack("Image/MachineIcons/EDM.png") });
            //Machines.Add(new MachineNode { Id = "EDM2", DisplayName = "EDM2", X = 550, Y = 50, Width = 150, Height = 200, IconPath = Pack("Image/MachineIcons/EDM.png") });
            //Machines.Add(new MachineNode { Id = "EDM3", DisplayName = "EDM3", X = 170, Y = 50, Width = 150, Height = 200, IconPath = Pack("Image/MachineIcons/EDM.png") });

            //Machines.Add(new MachineNode { Id = "ROBOT", DisplayName = "R", X = 350, Y = 250, Width = 150, Height = 150, IconPath = Pack("Image/MachineIcons/Robot.png") });

            //Machines.Add(new MachineNode { Id = "Track", DisplayName = "", X = 0, Y = 400, Width = 760, Height = 80, IconPath = Pack("Image/MachineIcons/long-track.png") });
            //Machines.Add(new MachineNode { Id = "ASE1", DisplayName = "ASE1", X = 120, Y = 470, Width = 150, Height = 150, IconPath = Pack("Image/MachineIcons/FMS.png") });
            //Machines.Add(new MachineNode { Id = "EW1", DisplayName = "EW1", X = 500, Y = 470, Width = 150, Height = 150, IconPath = Pack("Image/MachineIcons/Magzine.png") });



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
