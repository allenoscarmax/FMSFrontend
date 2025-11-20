using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq; // 需要
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

            foreach (var m in Machines)
            {
                var active = string.Equals(m.Id, RobotAtId, StringComparison.OrdinalIgnoreCase);
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
            if (track != null && robot.Height > 0)
                targetY = track.Y - robot.Height + 1;

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


        public FactoryLayoutViewModel()
        {
            UpdateHighlight(); // 初始化一次
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
