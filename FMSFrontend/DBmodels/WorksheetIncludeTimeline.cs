using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSCARMAXFMS_V3.DBmodels
{
    // 工單（含時間軸）資料模型：繼承 Worksheets，補齊各製程節點的時間點
    // 備註：時間是否為本地或 UTC，請與系統其餘時間欄位維持一致（建議統一為 UTC 並於前端轉時區）
    public class WorksheetIncludeTimeline : Worksheets
    {
        public DateTime? setupTime { get; set; } // 治具/工件上機（設置完成）時間

        public DateTime? dispatchTime { get; set; } // 派工時間（進入執行佇列/下達命令）

        public List<DateTime?> eDMStartTime { get; set; } = new List<DateTime?>(); // EDM 加工開始時間（可能多段/多刀序）
        public List<DateTime?> eDMEndTime   { get; set; } = new List<DateTime?>(); // EDM 加工結束時間（與 Start 對應，同索引為同一段）

        public List<DateTime?> cleaningStartTime { get; set; } = new List<DateTime?>(); // 清洗開始時間（多次清洗用多筆）
        public List<DateTime?> cleaningEndTime   { get; set; } = new List<DateTime?>(); // 清洗結束時間（與 Start 對應）

        public List<DateTime?> measuringStartTime { get; set; } = new List<DateTime?>(); // 量測開始時間（多次量測用多筆）
        public List<DateTime?> measuringEndTime   { get; set; } = new List<DateTime?>(); // 量測結束時間（與 Start 對應）

        public DateTime? completedTime { get; set; } // 工單完成時間（全流程完成）
        public DateTime? failedTime    { get; set; } // 工單失敗/中止時間
    }
}
