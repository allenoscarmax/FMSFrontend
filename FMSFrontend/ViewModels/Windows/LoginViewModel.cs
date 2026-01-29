using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FMSFrontend.ViewModels.Windows
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty] private string name = string.Empty;
        [ObservableProperty] private string password = string.Empty;
        [ObservableProperty] private string message = "";

        public List<LoginInfo> Workers { get; private set; } = new();

        public LoginViewModel()
        {
            // 這裡可以初始化其他東西，Workers 預設為空 List
        }

        /// <summary>
        /// 由 WindowService 呼叫，注入員工清單與預設帳號。
        /// </summary>
        public void Initialize(List<LoginInfo> workers, string initialUserName)
        {
            Workers = workers ?? new List<LoginInfo>();

            if (!string.IsNullOrWhiteSpace(initialUserName))
                Name = initialUserName;
        }

        [RelayCommand]
        private void LoginClick()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                Message = "請輸入使用者名稱";
                return;
            }
            if (string.IsNullOrWhiteSpace(Password))
            {
                Message = "請輸入密碼";
                return;
            }
            var user = Workers.FirstOrDefault(w => w.Name == Name);
            if (user == null)
            {
                Message = "使用者名稱不存在";
                return;
            }
            if (user.Password != Password)
            {
                Message = "密碼錯誤";
                return;
            }
            if (Application.Current.Windows != null)
            {
                foreach (Window w in Application.Current.Windows)
                {
                    if (w.DataContext == this)
                    {
                        w.DialogResult = true;
                        w.Close();
                        break;
                    }
                }
            }
        }
    }
    public class LoginInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
