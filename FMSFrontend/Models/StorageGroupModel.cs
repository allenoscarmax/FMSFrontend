using CommunityToolkit.Mvvm.ComponentModel;
using ControlzEx.Standard;
using FMSFrontend.Controls;
using FMSFrontend.Features.Dtos;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq; // ← for PageList
using System.Windows.Media;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;
namespace FMSFrontend.Models
{
    public enum MaterialType { None, Electrode, Workpiece, Probe }
    public enum ResultStatus { CheckSuccess, Checking, CheckFail } //尚未使用
    public enum CheckStatus { Checked, Unchecked } //尚未使用
    public partial class StorageGroupModel : ObservableObject
    {
        // 統計
        [ObservableProperty] public int waitingTotal = 0;      //待加工總數
        [ObservableProperty] public int processingTotal = 0;   //加工中總數
        [ObservableProperty] public int errorTotal = 0;        //異常總數
        [ObservableProperty] public int completedTotal = 0;    //已完成總數
        [ObservableProperty] public int restrictionTotal = 0;  //鎖定總數
        [ObservableProperty] public int bookedTotal = 0;       //預約總數
        //所有儲存庫
        [ObservableProperty] private ObservableCollection<StorageModel> storage = new();
        [ObservableProperty] private StorageModel selectStorage = new();

        // === StorageDetailViewModel ===
    }
    public partial class StorageModel : ObservableObject
    {
        // 讀取參數
        [ObservableProperty] private string name = string.Empty;
        [ObservableProperty] private string number = string.Empty;
        // [ObservableProperty] private string serial = string.Empty; // 材料庫序號
        // 尺寸
        [ObservableProperty] private int rows = 0;    // 最大列數
        [ObservableProperty] private int columns = 0; // 最大行數
        // 內容
        [ObservableProperty] private ObservableCollection<Slot> slots = new();
        // 統計
        [ObservableProperty] private int waitingCount = 0;     // 待加工數
        [ObservableProperty] private int processingCount = 0;  // 加工中數
        [ObservableProperty] private int errorCount = 0;       // 異常數
        [ObservableProperty] private int completedCount = 0;   // 已完成數
        [ObservableProperty] private int restrictionCount = 0; // 鎖定總數
        [ObservableProperty] private int bookedCount = 0;      // 預約總數
        // 自動計算屬性
        public string Title => Name + Number;                   // 材料庫顯示名稱

        public MaterialType Kind =>                             // 材料庫種類判斷
            (Name?.Contains("E") == true) ? MaterialType.Electrode :
            (Name?.Contains("W") == true) ? MaterialType.Workpiece :
            MaterialType.None;
        public Brush KindBrush => Kind switch                   // 材料庫顏色判斷
        {
            MaterialType.Workpiece => new SolidColorBrush(Color.FromRgb(0xE0, 0x8E, 0x45)), // 工件(橘)
            MaterialType.Electrode => new SolidColorBrush(Color.FromRgb(0x27, 0x79, 0xA7)), // 電極(藍)
            MaterialType.Probe => new SolidColorBrush(Color.FromRgb(0xE0, 0x8E, 0x45)),     // 探針(藍) 歸類在電極庫中
            _ => new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)) // 其他(白色)
        };

        partial void OnNameChanged(string value)
        {
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Kind));
            OnPropertyChanged(nameof(KindBrush));
        }
        partial void OnNumberChanged(string value) => OnPropertyChanged(nameof(Title));
    }
    public partial class Slot : ObservableObject
    {
        [ObservableProperty] private MaterialType kind = MaterialType.None;
        [ObservableProperty] private string id = "";
        [ObservableProperty] private string serial = "";
        public string Name = "";                                        // 名稱 (全名)
        [ObservableProperty] private string shortName = "";             // 名稱 (顯示用)
        [ObservableProperty] private string materialStatus = "";        // 材料狀態
        [ObservableProperty] private bool materialRestriction;  // 材料是否有鎖定
        [ObservableProperty] private string storageStatus = "";         // 材料庫是否預約
        [ObservableProperty] private bool storageRestriction;           // 材料庫是否有鎖定
        public Brush StatusBrush
        {
            get
            {
                // ① Restriction：最高優先權
                if (StorageRestriction)
                {
                    return new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0));
                }

                // ② 預約狀態（Reserved）
                if (StorageStatus == "Booked")
                {
                    return Brushes.White; // 米色
                }

                // ③ 一般材料狀態
                return MaterialStatus switch
                {
                    "Verified" => new SolidColorBrush(Color.FromRgb(0xE6, 0xB9, 0x3E)),
                    "Working" => new SolidColorBrush(Color.FromRgb(0x56, 0xC0, 0x6C)),
                    "Error" => new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B)),
                    "Completed" => new SolidColorBrush(Color.FromRgb(0x2F, 0x64, 0xCF)),
                    _ => new SolidColorBrush(Color.FromRgb(245, 245, 220))
                };
            }
        }
        partial void OnStorageRestrictionChanged(bool value) => OnPropertyChanged(nameof(StatusBrush));
        partial void OnMaterialStatusChanged(string value) => OnPropertyChanged(nameof(StatusBrush));
        partial void OnStorageStatusChanged(string value) => OnPropertyChanged(nameof(StatusBrush));

        string Code => Kind switch
        {
            MaterialType.Electrode => "E",
            MaterialType.Probe => "P",
            MaterialType.Workpiece => "W",
            _ => ""
        };
        partial void OnKindChanged(MaterialType value) => OnPropertyChanged(nameof(Code));

        public string StorageName { get; set; } = "";  // 倉線/倉號
        public int StorageNumber { get; set; }   // 倉線/倉號
        public int Region { get; set; } //現有：區域
        public int Row { get; set; }    // 行
        public int Col { get; set; }    // 列

        public string SlotCode = "";
        public string Worksheet = "";
        public string Program = ""; // 使用程式名稱
        public string Location = "";

        // 影響 StatusBrush 的來源變更時，主動通知
    }
}



















