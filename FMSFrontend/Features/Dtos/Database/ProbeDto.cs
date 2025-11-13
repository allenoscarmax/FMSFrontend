using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Dtos
{
    public class ProbeDto
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = "";
        public string tagSerial { get; set; } = "";
        public string probeName { get; set; } = "";
        public string probeType { get; set; } = "";
        public string currentLocation { get; set; } = "";
        public string state { get; set; } = "";
        public bool? restriction { get; set; }
        public string pairedEDM { get; set; } = "";

    }

    public enum ProbeState
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
    public enum ProbeCurrentLocation
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
}
