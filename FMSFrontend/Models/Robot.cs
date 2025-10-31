using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media;

namespace FMSFrontend.Models
{
    public partial class Robot : ObservableObject
    {
        
        [ObservableProperty] private string currentLocation = "";

        [ObservableProperty] private bool isRobotConnected; // 機器人連線狀態
        [ObservableProperty] private string name = "";
        [ObservableProperty] private Brush statusBrush = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55)); // 灰色 
        [ObservableProperty] private string currentAction = ""; // 目前動作
        [ObservableProperty] private string nextAction = ""; // 下一步動作
        [ObservableProperty] private string tagSerial = ""; //序號
        [ObservableProperty] private string materialKind = ""; //材料
        [ObservableProperty] private string materialName = ""; //材料名稱
      

        [ObservableProperty] private string selectedRobotIndexDisplay = "";
        [ObservableProperty] private bool isMultipleRobotVisible;
    }
}
