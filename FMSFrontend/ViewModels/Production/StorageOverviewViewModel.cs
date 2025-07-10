using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FMSFrontend.ViewModels.Production
{
    public partial class StorageOverviewViewModel : ObservableObject
    {
        private readonly ProductionLinesViewModel _parent;

        public ObservableCollection<StorageUnitViewModel> StorageUnits { get; set; } = new();

        public StorageOverviewViewModel(ProductionLinesViewModel parent)
        {
            _parent = parent;

            StorageUnits.Add(new StorageUnitViewModel("ES1", 6, 8));
            StorageUnits.Add(new StorageUnitViewModel("ES2", 6, 8));
            StorageUnits.Add(new StorageUnitViewModel("W1", 2, 5));
            StorageUnits.Add(new StorageUnitViewModel("W1", 2, 5));
            StorageUnits.Add(new StorageUnitViewModel("W1", 2, 5));

            StorageUnits.Add(new StorageUnitViewModel("W1", 2, 5));
            // ... 其他倉儲
        }

        [RelayCommand]
        private void ToggleExpand()
        {
            // 假設你要展開到特定 StorageId 的 DetailControl
            _parent.ShowDetail("ES1");
        }
    }



    public class StorageUnitViewModel
    {
        public string StorageName { get; set; } = "ES1";
        public int Rows { get; set; }
        public int Columns { get; set; }
        public ObservableCollection<StorageSlotViewModel> Slots { get; set; } = new();

        public Brush HeaderColor => StorageName.StartsWith("ES")
    ? new SolidColorBrush(Color.FromRgb(0x27, 0x79, 0xA7)) // 電極倉 = 藍色
    : new SolidColorBrush(Color.FromRgb(0xE0, 0x8E, 0x45)); // 工件倉 = 橘色

        public StorageUnitViewModel(string name, int rows, int cols)
        {
            StorageName = name;
            Rows = rows;
            Columns = cols;

            // 產生假資料（你未來可改為實際傳入）
            for (int i = 0; i < rows * cols; i++)
            {
                Slots.Add(new StorageSlotViewModel
                {
                    Status = i == 0 ? "Verified" :
                             i == 1 ? "Working" :
                             i == 2 ? "Error" :
                             i == 3 ? "Completed" :
                             i == 4 ? "Empty" :
                             "Reserved"
                });
            }

        }
    }

    public class StorageSlotViewModel
    {
        public string Status { get; set; } = "Verified"; // Verified / Working / Error / Completed / Reserved / Empty
        public bool IsDisabled { get; set; }
        public bool IsReserved { get; set; }

        // 可加上自動轉換色彩的屬性（或透過 Converter）
        public Brush Background => Status switch
        {
            "Verified" => Brushes.Gold,
            "Working" => Brushes.Green,
            "Error" => Brushes.IndianRed,
            "Completed" => Brushes.RoyalBlue,
            "Reserved" => Brushes.Gray,
            _ => Brushes.White
        };

    }


}
