using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Dtos
{
    public class FanucCNCDto
    {
        [JsonPropertyName("cNC_Operation_Mode")]
        public string CNC_Operation_Mode { get; set; } //CNC 運作模式

        [JsonPropertyName("cNC_light")]
        public int CNC_light { get; set; } //CNC 色燈  1:綠 2:黃 3:紅 0:沒亮
        [JsonPropertyName("axisX")]
        public double AxisX { get; set; }
        [JsonPropertyName("axisY")]
        public double AxisY { get; set; }
        [JsonPropertyName("axisZ")]
        public double AxisZ { get; set; }
        [JsonPropertyName("feedSpeed")]
        public int FeedSpeed { get; set; } //進給速度
        [JsonPropertyName("spindleSpeed")]
        public int SpindleSpeed { get; set; } //主軸轉速

        [JsonPropertyName("cNC_Connection")]
        public bool CNC_Connection {  get; set; } // CNC 連線狀態

        [JsonPropertyName("canControl")]
        public bool CanControl { get; set; }  // 是否允許控制 CNC（例如遠端啟動/停止），預設為 false
        [JsonPropertyName("starttoMill")]
        public bool StarttoMill { get; set; } // 是否已開始銑削，預設為 false
    }
}
