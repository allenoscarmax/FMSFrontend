using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Dtos
{
    public class WorksheetsTimelineDto
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("worksheetid")]
        public string WorkSheetId { get; set; } = "";

        [JsonPropertyName("worksheetserial")]
        public string WorkSheetSerial { get; set; } = ""; //工單編號

        [JsonPropertyName("workcommand")]
        public string WorkCommand { get; set; } = ""; //狀態

        [JsonPropertyName("timestampe")]
        public DateTime TimeStampe { get; set; } // 前端仍維持本地時間顯示

        [JsonPropertyName("electrodeserial")]
        public string ElectrodeSerial { get; set; } = "";

        [JsonPropertyName("edmnumber")]
        public string EDMnumber { get; set; } = ""; //機台名稱
    }

    public enum WorksheetTimelineWorkCommand
    {
        [Description("起單")]
        Setup,
        [Description("已派工")]
        Dispatched,
        [Description("被清洗機清洗中")]
        Cleaning,
        [Description("被EDM加工")]
        MachinedbyEDM,
        [Description("被清洗機清洗結束")]
        CleaningEnd,
        [Description("被EDM加工完成")]
        CompletedByEDM,
        [Description("被CMM量測中")]
        Measuring,
        [Description("被CMM量測完成")]
        MeasurementEnd,
        [Description("暫停")]
        Paused,
        [Description("失敗")]
        Failed,
    }
}
