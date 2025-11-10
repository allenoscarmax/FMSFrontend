using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.Models
{
    public partial class GlobalProperties : ObservableObject
    {
        public string _SplashScreenMessage = "None";  //None
        public string SplashScreenMessage { get { return _SplashScreenMessage; } set { _SplashScreenMessage = value; } }

        [ObservableProperty] private bool _isServerAlive;
        [ObservableProperty] private long _lastLatencyMs;     // 可選：顯示延遲
        [ObservableProperty] private DateTime _lastCheckedAt; // 可選：顯示最後檢查時間
    }
}
