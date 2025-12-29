using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Dtos.Apps
{
    public class UploadWorkOrderRequestDto
    {
        public string selectedEdm { get; set; }
        public string selectedCoordinate { get; set; }
        public string setupUser { get; set; } = "admin";

        public List<UploadWorkItemDto> workItems { get; set; }
        public List<UploadElectrodeItemDto> electrodeItems { get; set; }

        // ★ 新增：電極 / 工件 CSV 檔完整路徑（可以為 null / 空）
        public string electrodeCsvPath { get; set; }
        public string workpieceCsvPath { get; set; }
    }

    public class UploadWorkItemDto
    {
        public string workpieceName { get; set; }
        public string measurementProgram { get; set; }
        public string wrokpieceProgramFolderPath { get; set; }
    }

    public class UploadElectrodeItemDto
    {
        public string electrodeName { get; set; }
        public string measurementProgram { get; set; }
        public string electrodeProgramFolderPath { get; set; }

        public int? lifeTimes { get; set; }
        public int? offsetStatus { get; set; }
        public bool share { get; set; }
        public string shareElectrode { get; set; }
        public string shareId { get; set; }
    }

    public class UploadWorkOrderResultDto
    {
        public string worksheetNumber { get; set; }
        public bool success { get; set; }
        public string message { get; set; }

        // 新增：程式未上傳的機台（離線 / share 不可用）
        public List<string> programSkippedMachines { get; set; }

        // 可選：有成功上傳的機台
        public List<string> programUploadedMachines { get; set; }
    }
}
