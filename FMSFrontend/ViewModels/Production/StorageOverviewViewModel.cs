using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Standard;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Models;
using FMSFrontend.Services;
using FMSFrontend.ViewModels.Windows;
using MahApps.Metro.Controls;
using OSCARMAXFMS_V3.DBmodels;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using static FMSFrontend.ViewModels.ElectrodeDetailViewModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FMSFrontend.ViewModels.Production
{
    public partial class StorageOverviewViewModel : ObservableObject
    {
        private readonly StorageStore _storageStore;
        public StorageGroupModel StorageGroup => _storageStore.StorageGroup;

        private readonly ProductionLinesViewModel _parent;
        private readonly IHttpService _httpService;

        public StorageOverviewViewModel(ProductionLinesViewModel parent, IHttpService httpService, StorageStore storageStore)
        {
            _parent = parent;
            _httpService = httpService;

            _storageStore = storageStore;
            // 啟動即載入
            // _ = LoadStorageAsync();
        }

        [RelayCommand]
        private void ToggleExpand()
        {
            _parent.ShowDetail("ES1");
        }

        [RelayCommand]
        private void OpenMaterial(Slot slot)
        {
            _parent.OpenMaterial(slot.Serial);
        }
    }
    /*
    public class StorageUnitViewModel
    {
        public string StorageName { get; set; } = "ES1";
        public int Rows { get; set; }
        public int Columns { get; set; }
        public ObservableCollection<StorageSlotViewModel> Slots { get; set; } = new();

        public Brush HeaderColor => StorageName.IndexOf("E") == 0
            ? new SolidColorBrush(Color.FromRgb(0x27, 0x79, 0xA7)) // 電極倉 = 藍色
            : new SolidColorBrush(Color.FromRgb(0xE0, 0x8E, 0x45)); // 工件倉 = 橘色

        public StorageUnitViewModel(string name, int rows, int cols)
        {
            StorageName = name;
            Rows = rows;
            Columns = cols;
        }
    }

    public partial class StorageSlotViewModel : ObservableObject
    {
        // 讓 Status/IsDisabled/IsReserved 有變更通知（CommunityToolkit 會產生公開屬性）
        [ObservableProperty]
        private string status = "";

        [ObservableProperty]
        private bool isDisabled;

        // IsReserved 會被設定於建立 slot 時，也可能於後續變更
        [ObservableProperty]
        private bool isReserved;

        // 對應 XAML 中使用的綁定名：IsLocked
        // 回傳目前的 IsReserved 狀態
        public bool IsLocked => IsReserved;

        // 當 IsReserved 改變時，同步通知 IsLocked 也要更新 UI
        partial void OnIsReservedChanged(bool value)
        {
            OnPropertyChanged(nameof(IsLocked));
        }

        public Brush Background => Status switch
        {
            "Verified" => new SolidColorBrush(Color.FromRgb(0xE6, 0xB9, 0x3E)), // 待加工 (黃)
            "Working" => new SolidColorBrush(Color.FromRgb(0x56, 0xC0, 0x6C)),  // 加工中 (綠)
            "Error" => new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B)),    // 異常 (紅)
            "Completed" => new SolidColorBrush(Color.FromRgb(0x2F, 0x64, 0xCF)),// 完成 (藍)
            "Reserved" => new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)), // 保留/預約 (灰)
            "Empty" => Brushes.White,
            _ => Brushes.White
        };

        public MaterialRef Material { get; set; } = new MaterialRef();

        public bool IsElectrode { get; set; }
        public int Line { get; set; }   // 倉線/倉號
        public int Row { get; set; }    // 行
        public int Col { get; set; }    // 列
        public int Layer { get; set; } = 1;

        public string SlotCode => $"{(IsElectrode ? "E" : "W")}:{Line}:{Row}:{Col}:{Layer}";
    }
     */
    public class MaterialRef
    {
        public MaterialKind Kind { get; set; }
        public ElectrodeModel Electrode { get; set; } = new ElectrodeModel();
        public WorkpieceModel Workpiece { get; set; } = new WorkpieceModel();
        public IEnumerable<TimelineItemModel> Timeline { get; set; } = Enumerable.Empty<TimelineItemModel>();
    }
   
}
