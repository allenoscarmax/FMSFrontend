using CommunityToolkit.Mvvm.ComponentModel;
using ControlzEx.Standard;
using FMSFrontend.Controls;
using FMSFrontend.Features.Dtos;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq; // ← for PageList
using System.Windows.Media;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
namespace FMSFrontend.Models
{
    public class TimelineItemModel
    {
        public string WorkCommand = "";
        public string Text => WorkCommand switch
        {
            "Setup" => "起單",
            "Dispatched" => "已派工",
            "Cleaning" => "被清洗機清洗中",
            "MachinedbyEDM" => "被EDM加工",
            "CleaningEnd" => "被清洗機清洗結束",
            "CompletedByEDM" => "被EDM加工完成",
            "Measuring" => "被CMM量測中",
            "MeasurementEnd" => "被CMM量測完成",
            "Paused" => "暫停",
            "Failed" => "失敗",
            _ => WorkCommand,
        };
        public DateTime Time { get; set; }
        public string Status { get; set; } = "";
    }
}



















