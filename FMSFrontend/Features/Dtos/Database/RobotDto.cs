using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Dtos
{
    public class RobotDto
    {
        public string _id { get; set; } = "";
        public string robotCode { get; set; } = "";
        public int productionLine { get; set; } 
        public string robotNumber { get; set; } = ""; //
        public string robotName { get; set; } = "";  //設備名稱
        public string robotType { get; set; } = ""; //設備類型
        public string robot_IP { get; set; } = "";
        public string status { get; set; } = "";  //機台狀態
        public string image { get; set; } = "";
        public string onDeckObjSerial { get; set; } = "";
        public string setupUser { get; set; } = "";
    }

    public enum Robotstatus
    {
        [Description("離線中")]
        Disconnection = 0,
        [Description("運作中")]
        Running = 1,
        [Description("閒置中")]
        Stopping = 2,
        [Description("急停中")]
        EmergencyStop = 3
    }
}
