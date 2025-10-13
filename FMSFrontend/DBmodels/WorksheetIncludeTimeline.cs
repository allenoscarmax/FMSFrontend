using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSCARMAXFMS_V3.DBmodels
{
    public class WorksheetIncludeTimeline : Worksheets
    {
        public DateTime? SetupTime { get; set; }
        public DateTime? DispatchTime { get; set; }
        public List<DateTime?> EDMStartTime { get; set; } = new List<DateTime?>();
        public List<DateTime?> EDMEndTime { get; set; } = new List<DateTime?>();

        public List<DateTime?> CleaningStartTime { get; set; } = new List<DateTime?>();
        public List<DateTime?> CleaningEndTime { get; set; } = new List<DateTime?>();
        public List<DateTime?> MeasuringStartTime { get; set; } = new List<DateTime?>();
        public List<DateTime?> MeasuringEndTime { get; set; } = new List<DateTime?>();


        public DateTime? CompletedTime { get; set; }
        public DateTime? FailedTime { get; set; }
    }
}
