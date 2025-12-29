using CommunityToolkit.Mvvm.ComponentModel;

namespace FMSFrontend.Models
{
    public partial class UserSession : ObservableObject
    {
        [ObservableProperty]
        private bool isLoggedIn;

        [ObservableProperty]
        private string userName = string.Empty;

        public void SignIn(string userName)
        {
            UserName = userName;
            IsLoggedIn = true;
        }

        public void SignOut()
        {
            UserName = string.Empty;
            IsLoggedIn = false;
        }
    }
}
