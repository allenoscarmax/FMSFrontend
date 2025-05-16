using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media;

namespace FMSFrontend.Models
{
    public partial class Robot : ObservableObject
    {
        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string currentLocation;

        [ObservableProperty]
        private string currentAction;

        [ObservableProperty]
        private string nextAction;

        [ObservableProperty]
        private Brush statusRed;

        [ObservableProperty]
        private Brush statusYellow;

        [ObservableProperty]
        private Brush statusGreen;
    }
}
