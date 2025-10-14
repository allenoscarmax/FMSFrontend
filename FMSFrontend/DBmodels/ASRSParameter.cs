using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace MBModel
{
    /// <summary>
    /// ASRS 诀竟籔北╰参篈把计戈家
    /// </summary>
    public class ASRSParameter
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } // MongoDB ン醚絏

        public bool IsRobotConnected { get; set; }
        public bool IsRobotBusy { get; set; }
        public bool IsRobotError { get; set; }
        public bool IsRobotBePaused { get; set; }
        public bool IsASRSControlStop { get; set; }
        public bool IsRobotWorkingOnScaning { get; set; }
        public string? RobotPosition { get; set; }
        public int RobotStatus { get; set; }
        public bool BattAlarm { get; set; }
        public string? RobotAlarmMessage { get; set; }
        public int? RobotNumber { get; set; }
        public bool AsrsControlStart { get; set; }
        public bool AsrsControlPause { get; set; }
        public bool AsrsControlStop { get; set; }
        public bool AsrsProcessWarning { get; set; }
        public string? RobotDoingNow { get; set; }
        public string? RobotDoingNext { get; set; }
        public bool RobotMaintenanceNotice { get; set; }
        public bool RobotInSaftyArea { get; set; }
        public bool DispatchSwitch { get; set; }
        public bool DispatchNeedToStop { get; set; }
        public bool DispatchIsStoped { get; set; }
        public bool RobotCantReceiveNewTask { get; set; }
    }
}   