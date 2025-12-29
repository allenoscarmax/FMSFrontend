using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Dtos.Apps
{
    public class ReviseWorksheetRequestDto
    {
        /// <summary>工單號（你系統目前用 WorksheetNumber 當主鍵之一）</summary>
        public string worksheetNumber { get; set; } = "";

        public ReviseProcessAction action { get; set; }

        /// <summary>某些動作需要指定電極（例如 SelectNextElectrode 用）</summary>
        public string electrodeId { get; set; }

        /// <summary>稽核原因（建議 ForceComplete 必填）</summary>
        public string reason { get; set; }

        /// <summary>操作者（可用 JWT Claim 取代，這裡先比照 UploadWorkOrder）</summary>
        public string setupUser { get; set; } = "admin";
    }
    public class ReviseWorksheetResultDto
    {
        public string worksheetNumber { get; set; } = "";
        public bool success { get; set; }
        public string message { get; set; } = "";

    }

    public enum ReviseProcessAction
    {
        Cancel = 0,
        UndoElectrode = 1,        // 步驟-1 +（視需求）電極壽命-1或回滾
        SelectNextElectrode = 2,  // 換下一支電極
        ForceComplete = 3         // 強制完成
    }
}
