using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OSCARMAXFMS_V3.DBmodels
{
    public class Electrode
    {

        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string WorksheetNumber { get;set; }
        public string WorksheetDone { get; set; }
        public string TagSerial { get; set; }
        public string ElectrodeName { get; set; }
        public string ElectrodeType { get; set; }
        public string CurrentLocation { get; set; }
        public string State { get; set; }
        public string PairedEDM { get; set; }
        public int? OffsetStatus { get; set; } 
        //0: not offset, 1: 量測含加工 順序 量後加工量後加工, 2: 量測含加工 順序 全量後 在全加工 3:無量測載入補償加工模式
        public string Offset { get; set; } //會存xyzabc
        public bool ComplementUpload { get; set; } // 如果是狀態3才需要載入，載入後改為true
        public int? LifeTimes { get; set; }
        public int? UseTimes { get; set; }
        public string UnderSize { get; set; }
        public bool? Restriction { get; set; }
        public string EDMPGM { get; set; }
        public bool? Measuremented { get; set; }  //是否已量測 (狀態1或2才需要)
        public string MeasurementStatus { get; set; } //是否合格 (狀態1或2才需要)
        public string TempRetSLocation { get; set; }
        public string SetupUser { get; set; }

    }
}
