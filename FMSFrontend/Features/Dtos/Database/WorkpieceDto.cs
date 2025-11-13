using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel;


namespace FMSFrontend.Features.Dtos
{
    public class WorkpieceDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = "";

        public string tagSerial { get; set; } = "";
        public string worksheetNumber { get; set; } = "";
        public string workpieceName { get; set; } = "";
        public string status { get; set; } = "";
        public bool restriction { get; set; }
        public string pairedEDM { get; set; } = "";
        public string currentLocation { get; set; } = "";
        public string tempRetSLocation { get; set; } = ""; // WR:1:1:1:1
        public bool isCompleted { get; set; }
        public string edmpgm { get; set; } = "";          // ← 修正自 eDMPGM
        public bool? needInspect { get; set; }
        public bool? inspected { get; set; }
        public string inspectStatus { get; set; } = "";
        public string inspectOffset { get; set; } = "";
        public string setupUser { get; set; } = "";
    }

    public enum WorkpieceCurrentLocation
    {
        [Description("倉庫內")]
        inStore,
        [Description("手臂上")]
        onLoader,
        [Description("量測站上")]
        onMeasStation,
        [Description("清洗站上")]
        onCleaningStation,

        [Description("組裝站1上")]
        onASE1,
        [Description("組裝站2上")]
        onASE2,
        [Description("組裝站3上")]
        onASE3,


    }

    public enum WorkpieceState
    {
        [Description("無")]
        NA,  // 0
        [Description("新工件")]
        New,  // 1
        [Description("加工中")]
        Processing,  // 2
        [Description("待加工")]
        Verified,  // 3
        [Description("加工完成")]
        Completed,  // 4
        [Description("已預約")]
        Booked,  // 5
        [Description("異常")]
        Failure,  // 19
        [Description("無法辨認")]
        UnknownObject,  // 8
        [Description("待量測")]
        WaitMeasure,  // 9
        [Description("待清洗")]
        WaitWash,  // 10
        [Description("待回組裝站")]
        WaitReturnAssmblyStation,  // 11
        [Description("待回備料倉")]
        WaitReturnStorage,  // 12
        [Description("待回交換倉")]
        WaitReturnEXStorage,  // 13
        [Description("待回卸料倉")]
        WaitReturnUnloadingStorage,  // 14
        [Description("清洗完成")]
        WashCompleted,  // 15
        [Description("量測完成")]
        MeasureCompleted,  // 16
        [Description("量測中")]
        Measuring,  // 17
        [Description("清洗中")]
        Washing,  // 18
        [Description("待檢查")]
        Checking,  // 19

        [Description("已下架")]
        OffShelf = 100  // 100 (手動指定)
    }
    public enum InpectionStatus
    {
        [Description("無")]
        NA,  // 0
        [Description("組裝錯誤")]
        InspectError,
        [Description("組裝OK")]
        InspectOK,
    }
}
