using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace FMSFrontend.Models
{

    public partial class MagazinePara : ObservableObject
    {
        private static readonly SolidColorBrush PortTilieColor = new(Color.FromRgb(0x27, 0x79, 0xA7));
        private static readonly SolidColorBrush EleTilieColor = new(Color.FromRgb(0xE0, 0x8E, 0x45));
        public int magazineParasNumber = 2; //magazine參數數量

        [ObservableProperty] private bool isDoorLightOn = false; //日光燈
        [ObservableProperty] private ObservableCollection<MagazineParaInfo> magazineParas = new();
    }
    public partial class MagazineParaInfo : ObservableObject
    {
        // 新增 DoorId 供 UI 綁定（保持字串 StorageId 以符合既有 Control 的依賴屬性型別）
        [ObservableProperty] string storageId = string.Empty; // 與 doorId 對應，用於現有控件的文字顯示
        [ObservableProperty] Brush leftTitleBrush = Brushes.Gray;
        [ObservableProperty] string leftTitle = "";
        [ObservableProperty] Brush rightTitleBrush = Brushes.Gray;
        [ObservableProperty] string rightTitle = "";

        [ObservableProperty] Brush upperScanStatus = Brushes.Gray;
        [ObservableProperty] Brush upperDoorStatus = Brushes.Gray;
        [ObservableProperty] Brush lowerScanStatus = Brushes.Gray;
        [ObservableProperty] Brush lowerDoorStatus = Brushes.Gray;
    }
}
