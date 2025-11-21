using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FMSFrontend.Models
{
    public partial class RFIDBindModel : ObservableObject
    {
        public static readonly SolidColorBrush LightOn = new(Color.FromRgb(0x61, 0xB4, 0x55));
        public static readonly SolidColorBrush LightOff = new(Color.FromRgb(0x00, 0x00, 0x00));

        public DateTime? from = DateTime.Now; //過濾 起始日期 EX : 1/1
        public DateTime? to = DateTime.Now;   //過濾 結束日期 EX : 1/7
        [ObservableProperty] private ObservableCollection<BurnRecord> burnHistoryList = new(); //燒錄歷史
        [ObservableProperty] private Brush connectedBrush = LightOff; //連線狀態

        [ObservableProperty] private string tagSerial = ""; //標籤序號
        [ObservableProperty] private Brush tagBrush = LightOff; //標籤狀態

    }

    public class BurnRecord
    {
        public DateTime Time { get; set; }
        public string MaterialType { get; set; } = "";
        public string SerialNo { get; set; } = "";
        public string TagSerial { get; set; } = "";
    }
}
