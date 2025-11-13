using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FMSFrontend.Models;
namespace FMSFrontend.ViewModels
{


    public class TimelineItemViewModel
    {
        public TimelineItemViewModel() { }   // ← 加這個
        public string Text { get; set; } = "";
        public DateTime Time { get; set; }
        public string Status { get; set; } = ""; // "10%" 或 "✓"


        public TimelineItemViewModel(string text, DateTime time, string status)
        {
            Text = text;
            Time = time;
            Status = status;
        }

        // ★ 新增：吃一個 model 的建構子
        public TimelineItemViewModel(TimelineItemModel m)
        {
            Text = m.Text;
            Time = m.Time;
            Status = m.Status;
        }
    }

}
