using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FMSFrontend.Models
{
    public partial class ProductionLinesModel : ObservableObject
    {
        public bool? restriction { get; set; } //材料庫是否有限制  (true/false)
        public string status { get; set; } = "";//材料庫是否預約
        public List<Slot> SlotList { get; } = new();
    }
    public class StorageDetail //對應前端的 Slot格子
    {
        public bool isElectrode { get; set; } = false; //是否為電極
        public string Serial { get; set; } = ""; //材料序號
        public string Status { get; set; } = "";// 
         public bool? restriction { get; set; } //現有：是否有限制  (true/false)
    }
    public class StorageDetail //對應前端的 Slot格子
    {
        public bool isElectrode { get; set; } = false; //是否為電極
        public string Serial { get; set; } = ""; //材料序號
        public string Status { get; set; } = "";// 
        public bool? restriction { get; set; } //現有：是否有限制  (true/false)
    }
    public class SelectSlot 
    {
        public string Serial { get; set; } = ""; //材料序號
        public string id { get; set; } = ""; //材料id
        public bool isElectrode { get; set; } = false;
        public string Status { get; set; } = "";
        public bool? restriction { get; set; }
    }
}
