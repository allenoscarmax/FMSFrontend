using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.Models.Storage
{
    public class StorageUnitModel
    {
        public string StorageId { get; set; } = "";
        public int Rows { get; set; }
        public int Columns { get; set; }

        public List<SlotModel> Slots { get; set; } = new();
    }
}
