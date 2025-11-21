using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IniFile;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace FMSFrontend.ViewModels.Windows
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty] private string name = "";

        [ObservableProperty] private string password ="";

        public LoginViewModel()
        {
        }

        // 這裡可以放你的指令，例如處理按鈕點擊
        [RelayCommand]
        private void LoginClick()
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
