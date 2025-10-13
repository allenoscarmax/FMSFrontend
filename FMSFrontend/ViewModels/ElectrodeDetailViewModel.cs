using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.ViewModels
{
    public class ElectrodeDetailViewModel
    {
        public string ElectrodeName { get; set; }
        public string ElectrodeNo { get; set; }
        // ... 其他欄位
        public ElectrodeDetailViewModel(ElectrodeModel model)
        {
            ElectrodeName = model.Name;
            ElectrodeNo = model.No;
        }
        public class ElectrodeModel
        {
            public string Name { get; set; }
            public string No { get; set; }
            public string Type { get; set; }         // 方/圓/自定
            public string HolderNo { get; set; }     // 夾具代號
            public string TagSerial { get; set; }    // RFID 
        }
    }
}
