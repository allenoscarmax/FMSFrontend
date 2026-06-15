using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel;

namespace FMSFrontend.Features.Dtos
{
    public class ElectrodeDto
    {

        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = "";
        public string worksheetNumber { get;set; } = ""; //工單序號
        public string worksheetDone { get; set; } = "";
        public string tagSerial { get; set; } = ""; //序號
        public string electrodeName { get; set; } = ""; //電極名稱
        public string electrodeType { get; set; } = ""; //電極類型
        public string currentLocation { get; set; } = "";
        public string state { get; set; } = "";
        public string pairedEDM { get; set; } = "";
        public int? offsetStatus { get; set; } 
        //0: not offset, 1: 量測含加工 順序 量後加工量後加工, 2: 量測含加工 順序 全量後 在全加工 3:無量測載入補償加工模式
        public string offset { get; set; } = ""; //會存xyzabc
        public bool complementUpload { get; set; } // 如果是狀態3才需要載入，載入後改為true
        public int? lifeTimes { get; set; }
        public int? useTimes { get; set; }
        public string underSize { get; set; } = "";
        public bool? restriction { get; set; }
        public string edmpgm { get; set; } = ""; // ← 修正自 eDMPGM
        public string edM_offsetPGM { get; set; } = ""; // ← 修正自 eDM_offsetPGM
        public bool? measuremented { get; set; }  //是否已量測 (狀態1或2才需要)
        public string measurementStatus { get; set; } = "";   //是否合格 (狀態1或2才需要)
        public bool shared { get; set; }   // 是否被交棒
        public string shareLink { get; set; } = ""; // 放要share的電極完整名稱
        public string tempRetSLocation { get; set; } = "";
        public string setupUser { get; set; } = "";

    }

    public enum ElectrodeCurrentLocation
    {
        [Description("倉庫內")]
        inStore,
        [Description("在交換倉庫內")]
        inExchangeStore,
        [Description("在手臂背包內")]
        inBackpackStore,
        [Description("手臂上")]
        onLoader,
        [Description("量測站上")]
        onMeasStation,
        [Description("機台上")]
        onEDM,
    }
    
    public enum ElectrodeState
    {
        [Description("無")]
        NA = 0,
        [Description("加工中")]
        Processing = 1,
        [Description("待加工")]
        Verified = 2,
        [Description("已用罄")]
        Completed = 3,
        [Description("已預約")]
        Booked = 5,
        [Description("位置重疊")]
        Collided = 6,
        [Description("量測錯誤")]
        Faulty = 7,
        [Description("量測中")]
        Measuring = 8,
        [Description("位置異常")]
        Mislocated = 9,
        [Description("無法辨認")]
        UnknownObject = 10,
        [Description("待量測")]
        WaitMeasure = 11,
        [Description("待回備料倉")]
        WaitReturnStorage = 12,
        [Description("待回交換倉")]
        WaitReturnEXStorage = 13,
        [Description("待回卸料倉")]
        WaitReturnUnloadingStorage = 14,
        [Description("已下架")]
        OffShelf = 100
    }
}
