using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.ViewModels
{
    public class WorkpieceDetailViewModel
    {
        public string WorkName { get; set; }
        public string WorkNo { get; set; }
        // ... 其他欄位
        public WorkpieceDetailViewModel(WorkpieceModel model)
        {
            WorkName = model.Name;
            WorkNo = model.No;
        }
    }

    public class WorkpieceModel
    {
        public string Name { get; set; }         // 工件/零件名稱
        public string No { get; set; }           // 工件編號
        public string BatchNo { get; set; }
        public string RouteNo { get; set; }      // 途程 }
    }
}
