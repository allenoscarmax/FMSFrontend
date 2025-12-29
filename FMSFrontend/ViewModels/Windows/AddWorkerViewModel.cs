using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Windows;

namespace FMSFrontend.ViewModels.Windows
{
    // 用於新增/驗證工作人員的獨立 ViewModel，不再整合到 LoginViewModel。
    public partial class AddWorkerViewModel : ObservableObject
    {
        [ObservableProperty] private string title = string.Empty; // 員工帳號 / 編號
        [ObservableProperty] private string name = string.Empty; // 員工帳號 / 編號
        [ObservableProperty] private string password = string.Empty; // 密碼
        [ObservableProperty] private string message = string.Empty; // 密碼

        [ObservableProperty] private string confirmPassword = string.Empty; // 確認密碼
        [ObservableProperty] private bool isEnable; // 名稱欄位是否可編輯

        public List<LoginInfo> ExistingWorkers { get; private set; } = new();

        public AddWorkerViewModel()
        {
          
        }

        /// <summary>
        /// 執行時由 WindowService 呼叫，設定「新增 / 編輯」模式與已存在清單。
        /// </summary>
        public void Initialize(List<LoginInfo> workers, string selectName)
        {
            ExistingWorkers = workers ?? new List<LoginInfo>();

            if (string.IsNullOrWhiteSpace(selectName))
            {
                // 新增模式
                IsEnable = true;
                Title = "新增使用者";
                Name = string.Empty;
            }
            else
            {
                // 編輯模式
                IsEnable = false;              // 帳號不可改
                Title = "編輯使用者";
                Name = selectName;
            }
        }

        // 新增工作人員命令
        [RelayCommand]
        private void AddWorkerClick()
        {
            if (string.IsNullOrWhiteSpace(Name))     // 使用者名稱檢查
            {
                Message = "請輸入使用者名稱";
                return;
            }
            if (string.IsNullOrWhiteSpace(Password)) // 密碼檢查
            {
                Message = "請輸入密碼";
                return;
            }
            if (Password != ConfirmPassword)// 確認密碼檢查
            {
                Message = "兩次輸入的密碼不一致";
                return;
            }
            if (ExistingWorkers.Any(w => w.Name == Name) && IsEnable) // 只有在新增使用者時才檢查名稱是否存在
            {
                Message = "使用者名稱已存在";
                return;
            }
            // 這裡可改為呼叫後端 API 新增使用者
            ExistingWorkers.Add(new LoginInfo { Name = Name, Password = Password });
           // MessageBox.Show("新增成功");

            // 若此 ViewModel 綁定於對話視窗，成功後可自動關閉
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


}
