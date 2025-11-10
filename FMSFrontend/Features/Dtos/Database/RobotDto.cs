using System;
using System.Collections.Generic;
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
        public string robotNumber { get; set; } = "";
        public string robotName { get; set; } = "";
        public string robotType { get; set; } = "";
        public string robot_IP { get; set; } = "";
        public string status { get; set; } = "";
        public string image { get; set; } = "";
        public string onDeckObjSerial { get; set; } = "";
        public string setupUser { get; set; } = "";
    }
}
