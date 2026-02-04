using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Dtos.Apps
{
    public class FailWorksheetRequestDto 
    {
        public string worksheetNumber { get; set; } = "";
        public string reason { get; set; } = "";     // 可選：稽核用
        public string setupUser { get; set; } = "admin"; // 或改用 JWT Claim
    }
    public class FailWorksheetResultDto
    {
        public bool success { get; set; }
        public string message { get; set; } = "";
        public string worksheetNumber { get; set; } = "";
    }

}
