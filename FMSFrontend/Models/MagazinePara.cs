using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace FMSFrontend.Models
{

    public partial class MagazinePara : ObservableObject
    {
        private static readonly SolidColorBrush PortTilieColor = new(Color.FromRgb(0xE0, 0x8E, 0x45 ));
        private static readonly SolidColorBrush EleTilieColor = new(Color.FromRgb(0x27, 0x79, 0xA7));
        //日光燈
        [ObservableProperty] private bool isDoorLightOn = false;

        //標題
        [ObservableProperty] private ObservableCollection<Brush> leftTitleBrush = new ObservableCollection<Brush>(Enumerable.Repeat(EleTilieColor, 1));
        [ObservableProperty] private ObservableCollection<string> leftTitle = new ObservableCollection<string> (Enumerable.Repeat("電極", 1));
        [ObservableProperty] private ObservableCollection<Brush> rightTitleBrush = new ObservableCollection<Brush>(Enumerable.Repeat(PortTilieColor, 1));
        [ObservableProperty] private ObservableCollection<string> rightTitle = new ObservableCollection<string>(Enumerable.Repeat("工件", 1));
        //狀態燈
        [ObservableProperty] private ObservableCollection<Brush> upperScanStatus = new ObservableCollection<Brush>(Enumerable.Repeat(Brushes.Gray, 1));
        [ObservableProperty] private ObservableCollection<Brush> upperDoorStatus = new ObservableCollection<Brush>(Enumerable.Repeat(Brushes.Gray, 1));
        [ObservableProperty] private ObservableCollection<Brush> lowerScanStatus = new ObservableCollection<Brush>(Enumerable.Repeat(Brushes.Gray, 1));
        [ObservableProperty] private ObservableCollection<Brush> lowerDoorStatus = new ObservableCollection<Brush>(Enumerable.Repeat(Brushes.Gray, 1));
    }
}
