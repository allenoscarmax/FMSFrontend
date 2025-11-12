using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Dtos
{
    public class AsrsParameterDto
    {
        public bool isRobotConnected { get; set; }
        public bool isRobotBusy { get; set; }
        public bool isRobotError { get; set; }
        public bool isRobotBePaused { get; set; }
        public bool isASRSControlStop { get; set; }
        public bool isRobotWorkingOnScaning { get; set; }

        public string? robotPosition { get; set; }
        public int robotStatus { get; set; }
        public bool battAlarm { get; set; }
        public string? robotAlarmMessage { get; set; }
        public int? robotNumber { get; set; }

        public bool asrsControlStart { get; set; }
        public bool asrsControlPause { get; set; }
        public bool asrsControlStop { get; set; }
        public bool asrsProcessWarning { get; set; }

        public string? robotDoingNow { get; set; }
        public string? robotDoingNext { get; set; }

        public bool robotMaintenanceNotice { get; set; }
        public bool robotInSaftyArea { get; set; }

        public bool dispatchSwitch { get; set; }
        public bool dispatchNeedToStop { get; set; }
        public bool dispatchIsStoped { get; set; }
        public bool robotCantReceiveNewTask { get; set; }
    }
}
