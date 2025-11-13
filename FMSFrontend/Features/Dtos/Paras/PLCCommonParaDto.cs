using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Dtos
{
    public class PLCCommonParaDto
    {
        [JsonPropertyName("plcconnected")]
        public bool[] PLCConnected { get; set; } = new bool[3];

        [JsonPropertyName("estopswitch_fence")]
        public bool EStopSwitch_Fence { get; set; } = false; // 緊急開關4 ON表示壓下 OFF 鬆開(圍籬上)

        [JsonPropertyName("estopswitch_main")]
        public bool EStopSwitch_Main { get; set; } = false; // 緊急開關1 ON表示壓下 OFF 鬆開(中控)

        [JsonPropertyName("robot_air_alarm")]
        public bool Robot_air_alarm { get; set; } = false; // 手臂氣壓異常 ON異常 OFF 正常

        [JsonPropertyName("lubricator_alarm")]
        public bool Lubricator_Alarm { get; set; } = false; // 注油器異常 ON異常 OFF 正常

        [JsonPropertyName("fenceopen1")]
        public bool FenceOpen1 { get; set; } = false; // 圍籬開啟1

        [JsonPropertyName("estop")]
        public bool Estop { get; set; } = false; // 緊急開關按下 ON=異常 OFF=正常(M1-5)

        [JsonPropertyName("fenceopen")]
        public bool FenceOpen { get; set; } = false; // ON=圍籬開啟(M8-11)
    }

    public enum PLC_NUM
    {
        MainLineTrack = 0,
        Magzine = 1,
        WorkpieceAssemblyStation = 2,
    }
}
