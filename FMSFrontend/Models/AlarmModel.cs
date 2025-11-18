using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Data;
using System.Security.Cryptography.X509Certificates;

namespace FMSFrontend.Models
{
    public partial class AlarmGroupModel : ObservableObject
    {
        [ObservableProperty] public bool isAlarm = false; //是否有警報
        public bool IsAlarmWindowsOpen = false; //警報視窗是否開啟
        [ObservableProperty] public ObservableCollection<AlarmModel> alarmModels = new(); //警報清單
    }
    public partial class AlarmModel : ObservableObject
    {
        [ObservableProperty] private DateTime timeStamp = DateTime.Now; //<--
        [ObservableProperty] private string errorCode = ""; //<--
        [ObservableProperty] private string messageCn = ""; //<--
        [ObservableProperty] private string messageEn = ""; //<--
    }
}



















