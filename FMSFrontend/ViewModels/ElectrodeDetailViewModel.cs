using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.ViewModels
{
    public class ElectrodeDetailViewModel
    {
        public string JigSerial { get; set; } //治具序號
        public string ElectrodeName { get; set; } //電極名稱
        public string ElectrodeNo { get; set; } //電極編號
        public string ElectrodeType { get; set; } //電極類型
        public string Status { get; set; } //狀態
        public string HolderNo { get; set; } //電極夾定器標號
        public string TagSerial { get; set; } //Tag序號
        public string MaxDischargeCount { get; set; } //最大放電次數
        public string UsageRate { get; set; } //電極使用率
        public string Compensation { get; set; } //電極補償值
        public string ProcessedCount { get; set; }//被加工次數
        // ... 其他欄位
        public ElectrodeDetailViewModel(ElectrodeModel model)
        {
            JigSerial = model.JigSerial;
            ElectrodeName = model.Name;
            ElectrodeNo = model.No;
            ElectrodeType = model.Type;
            Status = model.Status;
            HolderNo = model.HolderNo;
            TagSerial = model.TagSerial;
            MaxDischargeCount   = model.MaxDischargeCount;
            UsageRate = model.UsageRate;
            Compensation = model.Compensation;
            ProcessedCount = model.ProcessedCount;
        }
        public class ElectrodeModel
        {
            public string JigSerial { get; set; } //治具序號
            public string Name { get; set; } //名稱
            public string No { get; set; } //編號
            public string Type { get; set; }         // 方/圓/自定
            public string Status { get; set; } //狀態
            public string HolderNo { get; set; }     // 夾具代號
            public string TagSerial { get; set; }    // RFID 
            public string MaxDischargeCount { get; set; } //最大放電次數
            public string UsageRate { get; set; } //電極使用率
            public string Compensation { get; set; } //電極補償值
            public string ProcessedCount { get; set; }//被加工次數
        }
    }
}
