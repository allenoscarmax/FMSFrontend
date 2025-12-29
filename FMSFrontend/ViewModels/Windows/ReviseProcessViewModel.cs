using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FMSFrontend.ViewModels
{
    public partial class ReviseProcessViewModel : ObservableObject
    {
       public ReviseProcessViewModel()
        {

        }


        [ObservableProperty]
        private ReviseProcessAction? selectedAction;

        [RelayCommand]
        private void UndoElectrode()
        {
            SelectedAction = ReviseProcessAction.UndoElectrode;
        }

        [RelayCommand]
        private void SelectNextElectrode()
        {
            SelectedAction = ReviseProcessAction.SelectNextElectrode;
        }

        [RelayCommand]
        private void ForceComplete()
        {
            SelectedAction = ReviseProcessAction.ForceComplete;
        }

        [RelayCommand]
        private void Cancel()
        {
            SelectedAction = ReviseProcessAction.Cancel;
        }
    }

    /// <summary>
    /// 流程修正結果
    /// </summary>
    public enum ReviseProcessAction
    {
        Cancel = 0,
        UndoElectrode = 1,
        SelectNextElectrode = 2,
        ForceComplete = 3
    }
}
