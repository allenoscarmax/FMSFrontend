using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FMSFrontend.Models;

namespace FMSFrontend.ViewModels
{
    public class ElectrodeDetailViewModel : INotifyPropertyChanged
    {
        public string Id { get; set; } = "";
        public string ElectrodeName { get; set; } //電極名稱
        public string ElectrodeNo { get; set; } //電極編號
        public string ElectrodeType { get; set; } //電極類型
        private string _status = ""; //狀態
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
                }
            }
        }
        public string TagSerial { get; set; } //Tag序號
        public string MaxDischargeCount { get; set; } //最大放電次數
        public string Compensation { get; set; } //電極補償值
        public string ProcessedCount { get; set; }//被加工次數
        public bool ElecRestriction { get; set; } // 新增：工件或電極 限制/鎖定
        public string StorageId { get; set; } = ""; // 新增： Storage 限制/鎖定
        public bool StorageRestriction { get; set; } // 新增： Storage 限制/鎖定
        //public string JigSerial { get; set; } //治具序號
        //public string HolderNo { get; set; } //電極夾定器標號
        //public string UsageRate { get; set; } //電極使用率

        // ... 其他欄位
        public ElectrodeDetailViewModel(ElectrodeModel model)
        {
            Id = model.Id;
            ElectrodeName = model.Name;
            ElectrodeNo = model.No;
            ElectrodeType = model.Type;
            _status = model.Status;
            TagSerial = model.TagSerial;
            MaxDischargeCount = model.MaxDischargeCount;
            Compensation = model.Compensation;
            ProcessedCount = model.ProcessedCount;
            ElecRestriction = model.ElecRestriction;
            StorageRestriction = model.StorageRestriction;
            //JigSerial = model.JigSerial;
            //HolderNo = model.HolderNo;
            //UsageRate = model.UsageRate;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
