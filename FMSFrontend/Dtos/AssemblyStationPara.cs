
using MongoDB.Bson.IO;

namespace OSCARMAXFMS_V3.Models
{
    public class AssemblyStationPara
    {

        public bool Air_error { get; set; } = false;
        public bool DoorisOpen { get; set; } = false; //門是否被打開 ON 打開 OFF 關閉中
        public bool RFIDisPolarization { get; set; } = false; //ON 表示回到安全位置，OFF 表示不在安全位置
        public bool WorkpieceOnAssemblyStation { get; set; } =  false; //ON 表示有工件，OFF 表示無工件
        public bool Notification_IncomingPart { get; set; } = false; //ON=手臂可取工件，OFF 表示無工件要進來
        public bool Notification_WaitingWorkpieceReturn { get; set; } = false; //ON=手臂可放工件，OFF 表示無工件要回去
        public bool Alarm { get; set; } = false; //ON=有警報，OFF 表示無警報

        public bool Require_IncomingPart { get; set; } = false; //UI要求進工件 UI -> 後台
        public bool Require_OutcomingPart { get; set; } = false; //UI要求出工件  UI -> 後台

        public bool Notification_Doorislocked { get; set; } = false; //PLC回饋 門已鎖好



    }
}
