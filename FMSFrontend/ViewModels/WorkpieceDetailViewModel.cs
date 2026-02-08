using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FMSFrontend.Models;

namespace FMSFrontend.ViewModels
{
    public class WorkpieceDetailViewModel : INotifyPropertyChanged
    {
        public string Id { get; set; } = "";
        public string JigSerial { get; set; } = "";
        public string WorkName { get; set; } = "";
        public string WorkNo { get; set; } = "";
        public string WorkType { get; set; } = "";
        public string PartNo { get; set; } = "";
        public string OrderNo { get; set; } = "";
        public string ClampNo { get; set; } = "";
        private string _status = "";
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
        public string BatchNo { get; set; } = "";
        public string PartName { get; set; } = "";
        public string SerialCode { get; set; } = "";
        public string RouteNo { get; set; } = "";
        public bool Restriction { get; set; } = false;
        public string StorageId { get; set; } = "";
        public bool StorageRestriction { get; set; }
        public WorkpieceDetailViewModel(WorkpieceModel model)
        {
            Id = model.Id ?? "";
            JigSerial = model?.JigSerial ?? "";
            WorkName = model?.Name ?? "";
            WorkNo = model?.No ?? "";
            WorkType = model?.WorkType ?? "";
            PartNo = model?.PartNo ?? "";
            OrderNo = model?.OrderNo ?? "";
            ClampNo = model?.ClampNo ?? "";
            _status = model?.Status ?? "";
            BatchNo = model?.BatchNo ?? "";
            PartName = model?.PartName ?? "";
            SerialCode = model?.SerialCode ?? "";
            RouteNo = model?.RouteNo ?? "";
            Restriction = model?.WorkRestriction ?? false;
            StorageRestriction = model?.StorageRestriction ?? false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
