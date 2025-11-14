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

        // 斷路器參數（可視需求調整）
        [ObservableProperty] private int _consecutiveFailures;    // 連續失敗數
        [ObservableProperty] private DateTime _openUntil;         // 開路到何時（冷卻時間）

        public bool IsOpen => DateTime.UtcNow < OpenUntil;       // 斷路器是否開路（拒絕）
        public bool IsHalfOpen => !IsOpen && !IsServerAlive;      // 半開：允許少量探測

        public void RecordSuccess()
        {
            ConsecutiveFailures = 0;
            IsServerAlive = true;
            OpenUntil = DateTime.MinValue;
        }

        public void RecordFailure(int threshold = 3, int coolDownSeconds = 15)
        {
            ConsecutiveFailures++;
            IsServerAlive = false;

            if (ConsecutiveFailures >= threshold)
                OpenUntil = DateTime.UtcNow.AddSeconds(coolDownSeconds); // 開路冷卻
        }
    }
}
