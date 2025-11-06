using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace FMSFrontend.Models
{
    public partial class RFIDBindData : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<BurnRecord> burnHistoryList = new();
    }

    public class BurnRecord
    {
        public DateTime Time { get; set; }
        public string MaterialType { get; set; } = "";
        public string SerialNo { get; set; } = "";
        public string TagSerial { get; set; } = "";
    }
}
