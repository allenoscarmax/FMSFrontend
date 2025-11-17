using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media;

namespace FMSFrontend.Models
{
    public enum AsrsControlState
    {
        Unknown = 0,
        Started,
        Paused,
        Stopped
    }
    public partial class Robot : ObservableObject
    {
        
        [ObservableProperty] private string currentLocation = "";

        [ObservableProperty] private bool isRobotConnected; // 機器人連線狀態
        [ObservableProperty] private string name = "Robot #1";



        [ObservableProperty] private Brush statusBrush = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55)); // 灰色 
        [ObservableProperty] private string currentAction = ""; // 目前動作
        [ObservableProperty] private string nextAction = ""; // 下一步動作

        [ObservableProperty] private string onDeckObjSerial = ""; //序號
        [ObservableProperty] private string materialKind = ""; //材料
        [ObservableProperty] private string materialName = ""; //材料名稱
        [ObservableProperty] private string materialShortName = ""; //材料簡稱(顯示用)

        //設備資訊
        [ObservableProperty] private string status = ""; // 灰色 
        [ObservableProperty] private string equipmentType = "2"; //設備類型
        [ObservableProperty] private string equipmentModel = ""; //設備型號
        [ObservableProperty] private string currentProgram = ""; //目前程式
        
        [ObservableProperty] private string selectedRobotIndexDisplay = "";
        [ObservableProperty] private bool isMultipleRobotVisible;

        // === 新增：ASRS 控制狀態（由三個布林收斂成一個 enum） ===
        [ObservableProperty] private AsrsControlState asrsState = AsrsControlState.Unknown;
        // 方便 XAML 綁定的只讀衍生布林
        public bool IsStarted => AsrsState == AsrsControlState.Started;
        public bool IsPaused => AsrsState == AsrsControlState.Paused;
        public bool IsStopped => AsrsState == AsrsControlState.Stopped;

        // 若上面三個要支援通知，就在 AsrsState 的 partial method 觸發：
        partial void OnAsrsStateChanged(AsrsControlState value)
        {
            OnPropertyChanged(nameof(IsStarted));
            OnPropertyChanged(nameof(IsPaused));
            OnPropertyChanged(nameof(IsStopped));
        }

        // === 新增：派工開關 ===
        [ObservableProperty] private bool dispatchEnabled;

    }
}
