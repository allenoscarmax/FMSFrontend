using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Dtos
{
    public class AssemblyStationParaDto
    {
        [JsonPropertyName("air_error")]
        public bool Air_error { get; set; } = false;

        [JsonPropertyName("doorisopen")]
        public bool DoorisOpen { get; set; } = false; // 門是否被打開 ON 打開 OFF 關閉中

        [JsonPropertyName("rfidispolarization")]
        public bool RFIDisPolarization { get; set; } = false; // ON 表示回到安全位置，OFF 表示不在安全位置

        [JsonPropertyName("workpieceonassemblystation")]
        public bool WorkpieceOnAssemblyStation { get; set; } = false; // ON 表示有工件，OFF 表示無工件

        [JsonPropertyName("notification_incomingpart")]
        public bool Notification_IncomingPart { get; set; } = false; // ON=手臂可取工件，OFF=無工件要進來

        [JsonPropertyName("notification_waitingworkpiecereturn")]
        public bool Notification_WaitingWorkpieceReturn { get; set; } = false; // ON=手臂可放工件，OFF=無工件要回去

        [JsonPropertyName("alarm")]
        public bool Alarm { get; set; } = false; // ON=有警報，OFF=無警報

        [JsonPropertyName("require_incomingpart")]
        public bool Require_IncomingPart { get; set; } = false; // UI要求進工件 (UI→後台)

        [JsonPropertyName("require_outcomingpart")]
        public bool Require_OutcomingPart { get; set; } = false; // UI要求出工件 (UI→後台)

        [JsonPropertyName("notification_doorislocked")]
        public bool Notification_Doorislocked { get; set; } = false; // PLC回饋 門已鎖好
    }
}
