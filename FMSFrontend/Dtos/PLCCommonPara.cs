using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSCARMAXFMS_V3.Models
{
    public class PLCCommonPara
    {
        public bool[] PLCConnected{ get; set; } = new bool[3];

        public bool EStopSwitch_Fence { get; set; } = false; //緊急開關4 ON表示壓下 OFF 鬆開(圍籬上)
        public bool EStopSwitch_Main { get; set; } = false; //緊急開關1 ON表示壓下 OFF 鬆開(中控)
        public bool Robot_air_alarm { get; set; } = false; //手臂氣壓異常 ON異常 OFF 正常
        public bool Lubricator_Alarm { get; set; } =  false;//注油器異常 ON異常 OFF 正常
        public bool FenceOpen1 { get; set; } = false; //圍籬開啟1

        public bool Estop { get; set; } = false; //緊急開關按下 ON 表示異常 OFF 正常(M1-5)
        public bool FenceOpen { get; set; } = false; //ON=圍籬開啟(M8-11)

    }
}
