using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FMSFrontend.Models
{
    public partial class ProductionLinesModel : ObservableObject
    {
        public bool? restriction { get; set; } //庫是否有限制  (true/false)
        public string status { get; set; } //工件/電極是否有限制  (true/false)

      //  public List<Slot> SlotList =  new();
    }
    class Slot : ObservableObject
    {
        public bool isElectrode { get; set; } = false; //是否為電極
        public string Serial { get; set; } = "";
        public string Status { get; set; } = "";
        public bool? restriction { get; set; } //現有：是否有限制  (true/false)


    }
    class SelectSlot : ObservableObject
    {
        public bool isElectrode { get; set; } = false; //是否為電極
        public string Serial { get; set; } = "";
        public string Status { get; set; } = "";
        public bool? restriction { get; set; } //現有：是否有限制  (true/false)

    }

}
