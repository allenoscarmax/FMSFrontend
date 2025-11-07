using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FMSFrontend.Features.Dtos
{
    public class ElectrodeDto
    {

        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = "";
        public string worksheetNumber { get;set; } = "";
        public string worksheetDone { get; set; } = "";
        public string tagSerial { get; set; } = "";
        public string electrodeName { get; set; } = "";
        public string electrodeType { get; set; } = "";
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
}
