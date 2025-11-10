using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Dtos
{
    public class WorksheetsTimelineDto
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("worksheetid")]
        public string WorkSheetId { get; set; }

        [JsonPropertyName("worksheetserial")]
        public string WorkSheetSerial { get; set; }

        [JsonPropertyName("workcommand")]
        public string WorkCommand { get; set; }

        [JsonPropertyName("timestampe")]
        public DateTime TimeStampe { get; set; } // 前端仍維持本地時間顯示

        [JsonPropertyName("electrodeserial")]
        public string ElectrodeSerial { get; set; }

        [JsonPropertyName("edmnumber")]
        public string EDMnumber { get; set; }
    }
}
