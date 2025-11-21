using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Data;
using System.Security.Cryptography.X509Certificates;

namespace FMSFrontend.Models
{
    public partial class StationModel : ObservableObject
    {
        [ObservableProperty] private string machineImagePath = ""; //機台圖片路徑
        [ObservableProperty] private bool air_error = false;
        [ObservableProperty] private bool doorisOpen = false; // 門是否被打開 ON 打開 OFF 關閉中
        [ObservableProperty] private bool rFIDisPolarization = false; // ON 表示回到安全位置，OFF 表示不在安全位置
        [ObservableProperty] private bool workpieceOnAssemblyStation = false; // ON 表示有工件，OFF 表示無工件
        [ObservableProperty] private bool notification_IncomingPart = false; // ON=手臂可取工件，OFF=無工件要進來
        [ObservableProperty] private bool notification_WaitingWorkpieceReturn = false; // ON=手臂可放工件，OFF=無工件要回去
        [ObservableProperty] private bool alarm = false; // ON=有警報，OFF=無警報
        [ObservableProperty] private bool require_IncomingPart = false; // UI要求進工件 (UI→後台)
        [ObservableProperty] private bool require_OutcomingPart = false; // UI要求出工件 (UI→後台)
        [ObservableProperty] private bool notification_Doorislocked = false; // PLC回饋 門已鎖好
    }
}



















