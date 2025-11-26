using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
// no need: using System.Windows.Input;

namespace FMSFrontend.ViewModels.Windows
{
    // 讓 XAML 的 {x:Static vm:ScheduleMode.*} 能找到
    public enum ScheduleMode { None, Daily, Weekly, Monthly }

    /// <summary>
    /// 內容區的選項（週一~週日、1~31、last）
    /// </summary>
    public class OptionItem : INotifyPropertyChanged
    {
        public int Value { get; }      // 週: 1~7 (Mon=1)；月: 1~31；0=last
        public string Label { get; } = ""; // "週一"、"30"、"last"

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { if (_isSelected != value) { _isSelected = value; OnPropertyChanged(); } }
        }

        private bool _isNegative;
        public bool IsNegative
        {
            get => _isNegative;
            set { if (_isNegative != value) { _isNegative = value; OnPropertyChanged(); OnPropertyChanged(nameof(Display)); } }
        }

        public string Display => IsNegative ? $"-{Label}" : Label;

        public OptionItem(int value, string label) { Value = value; Label = label; }

        // 事件改為可為 null（加上問號），或可改用初始化空 delegate（另一選項）
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    // ⭐ 用 partial，讓 [RelayCommand] 的 Source Generator 產生 *Command 屬性
    public partial class PeriodWindowViewModel : INotifyPropertyChanged
    {


        // 顯示在 TitleBar 的標題
        private string _windowTitle = "排程週期";
        public string WindowTitle { get => _windowTitle; set { _windowTitle = value; OnPropertyChanged(); } }

        // 模式
        private ScheduleMode _selectedMode = ScheduleMode.None;
        public ScheduleMode SelectedMode { get => _selectedMode; set { _selectedMode = value; OnPropertyChanged(); } }

        // 內容
        public ObservableCollection<OptionItem> WeeklyOptions { get; } = new();
        public ObservableCollection<OptionItem> MonthlyOptions { get; } = new();

        // 時間
        public ObservableCollection<int> Hours { get; } = new(Enumerable.Range(0, 24).ToList());
        public ObservableCollection<int> Minutes { get; } = new(Enumerable.Range(0, 60).ToList());

        private int _hour = 9;
        public int Hour { get => _hour; set { _hour = Math.Clamp(value, 1, 12); OnPropertyChanged(); } }

        private int _minute = 41;
        public int Minute { get => _minute; set { _minute = Math.Clamp(value, 0, 59); OnPropertyChanged(); } }

        private bool _isPm;
        public bool IsPm { get => _isPm; set { _isPm = value; OnPropertyChanged(); } }

        // 關閉由 Window 指定
        public Action<bool> CloseAction { get; set; } = _ => { };

        public PeriodWindowViewModel()
        {
            // 週一~週日 (Mon=1)
            string[] days = { "週日", "週一", "週二", "週三", "週四", "週五", "週六" };
            for (int i = 0; i < 7; i++) WeeklyOptions.Add(new OptionItem(i + 1, days[i]));

            // 1~31 + last=0
            for (int i = 1; i <= 31; i++) MonthlyOptions.Add(new OptionItem(i, i.ToString()));
            MonthlyOptions.Add(new OptionItem(0, "last"));
        }

        // ===== RelayCommand 版本（自動產生 SelectModeCommand、ToggleNegativeCommand、ConfirmCommand、CancelCommand） =====

        [RelayCommand]
        private void SelectMode(ScheduleMode mode) => SelectedMode = mode;

        [RelayCommand]
        private void ToggleNegative(OptionItem? item)
        {
            if (item != null) item.IsNegative = !item.IsNegative;
        }

        // 回傳結果（給呼叫端在關窗後取用）
        public ScheduleMode ConfirmedMode { get; private set; } = ScheduleMode.None;
        public int[] ConfirmedDays { get; private set; } = Array.Empty<int>();

        [RelayCommand]
        private void Confirm()
        {
            // 1) 設定回傳資料
            ConfirmedMode = SelectedMode;
            ConfirmedDays = SelectedMode switch
            {
                ScheduleMode.Weekly => CollectWeekly(),
                ScheduleMode.Monthly => CollectMonthly(),
                _ => Array.Empty<int>()
            };

            // 2) 關窗（由 Window 設定 CloseAction 來執行 DialogResult=true + Close）
            CloseAction(true);
        }

        [RelayCommand]
        private void Cancel() => CloseAction(false);


        // 讓外部設定預設值（SettingsPage 開窗前呼叫）
        public void ApplyInitial(ScheduleMode mode, int[] weekly, int[] monthly, int hour, int minute, bool isPm, string? title = null)
        {
            if (!string.IsNullOrWhiteSpace(title)) WindowTitle = title!;
            SelectedMode = mode;

            foreach (var it in WeeklyOptions) { it.IsSelected = false; it.IsNegative = false; }
            foreach (var it in MonthlyOptions) { it.IsSelected = false; it.IsNegative = false; }

            MarkInitial(WeeklyOptions, weekly);
            MarkInitial(MonthlyOptions, monthly);

            Hour = hour;
            Minute = minute;
            IsPm = isPm;
        }

        private static void MarkInitial(ObservableCollection<OptionItem> target, int[]? init)
        {
            if (init == null) return;
            var set = init.ToHashSet();
            foreach (var it in target)
            {
                if (set.Contains(it.Value) || set.Contains(-it.Value))
                {
                    it.IsSelected = true;
                    it.IsNegative = set.Contains(-it.Value);
                }
            }
        }

        // 讓呼叫端收集結果
        public int[] CollectWeekly() => WeeklyOptions.Where(x => x.IsSelected).Select(x => x.IsNegative ? -x.Value : x.Value).ToArray();
        public int[] CollectMonthly() => MonthlyOptions.Where(x => x.IsSelected).Select(x => x.IsNegative ? -x.Value : x.Value).ToArray();

        // 將事件宣告改為可為 null 以符合 nullable reference types
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
