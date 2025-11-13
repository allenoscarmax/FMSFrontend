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
using System.Text.RegularExpressions;
namespace FMSFrontend.Models
{
    public partial class MachineModel : ObservableObject
    {
        [ObservableProperty] private string machineName = "";
        [ObservableProperty] private string status = "idle"; // 可為 idle / running / warning / error / disabled
        [ObservableProperty] private string type = "";
        [ObservableProperty] private bool restriction;
    }
}



















