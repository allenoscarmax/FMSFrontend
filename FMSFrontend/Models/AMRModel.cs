using CommunityToolkit.Mvvm.ComponentModel;

namespace FMSFrontend.Models
{
    public partial class AMRModel : ObservableObject
    {
        [ObservableProperty] private bool success;
        [ObservableProperty] private string msg = "";
        [ObservableProperty] private string agvId = "";
        [ObservableProperty] private string status = "";
        [ObservableProperty] private string alarmsMsg = "";
        [ObservableProperty] private string battery = "";
        [ObservableProperty] private string location = "";
        [ObservableProperty] private string currentRoute = "";
        [ObservableProperty] private string workingStatus = "";
    }
}
