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
        public string Text { get; set; } = "";
        public DateTime Time { get; set; }
        public string Status { get; set; } = "";
    }
}



















