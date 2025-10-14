using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.ViewModels
{
    public class WorkpieceDetailViewModel
    {
        public string JigSerial { get; set; }
        public string WorkName { get; set; }
        public string WorkNo { get; set; }
        public string WorkType { get; set; }
        public string PartNo { get; set; }
        public string OrderNo { get; set; }
        public string ClampNo { get; set; }
        public string Status { get; set; }
        public string BatchNo { get; set; }
        public string PartName { get; set; }
        public string SerialCode { get; set; }
        public string RouteNo { get; set; }

        public WorkpieceDetailViewModel(WorkpieceModel model)
        {
            JigSerial = model?.JigSerial;
            WorkName = model?.Name;
            WorkNo = model?.No;
            WorkType = model?.WorkType;
            PartNo = model?.PartNo;
            OrderNo = model?.OrderNo;
            ClampNo = model?.ClampNo;
            Status = model?.Status;
            BatchNo = model?.BatchNo;
            PartName = model?.PartName;
            SerialCode = model?.SerialCode;
            RouteNo = model?.RouteNo;
        }
    }

    public class WorkpieceModel
    {
        public string JigSerial { get; set; }    // 治具序號
        public string Name { get; set; }         // 工件/零件名稱
        public string No { get; set; }           // 工件編號
        public string WorkType { get; set; }     // 工作類型
        public string PartNo { get; set; }       // 零件件號
        public string OrderNo { get; set; }      // 工單序號
        public string ClampNo { get; set; }      // 夾釘器編號
        public string Status { get; set; }       // 狀態
        public string BatchNo { get; set; }      // 批號
        public string PartName { get; set; }     // 零件名稱
        public string SerialCode { get; set; }   // 序號順序碼
        public string RouteNo { get; set; }      // 途程號碼
    }
}
