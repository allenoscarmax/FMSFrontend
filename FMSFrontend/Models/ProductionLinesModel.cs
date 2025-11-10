using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FMSFrontend.Models
{
    public partial class ProductionLinesModel : ObservableObject
    {
        public List<string> SerialList =  new();

    }
    class ProductionLineItem : ObservableObject
    {
        //材料(電極/工件)資訊
        public string Serial { get; set; } = "";
    
        public string Status { get; set; } = "";

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
        public string Status { get; set; } = "";
    }
}
