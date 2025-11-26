using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace FMSFrontend.ViewModels.Windows
{
    // 用於依據輸入的欄位資訊動態產生編輯介面的 ViewModel
    public partial class EditGridViewModel : ObservableObject
    {
        [ObservableProperty] private string title = "編輯";
        [ObservableProperty] private ObservableCollection<EditGridInfo> display = new();

        public EditGridViewModel(List<EditGridInfo> items, string title = "編輯")
        {
            // 初始化顯示的欄位列表
            if (items != null && items.Any())
            {
                Display = new ObservableCollection<EditGridInfo>(items);
            }
            Title = title ?? string.Empty;
        }

        // 完成編輯命令
        [RelayCommand]
        private void EditClick()
        {
            // 關閉視窗並回傳結果
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

        public class EditGridInfo
        {
            public string Title { get; set; } = string.Empty; // 標題
            public string Data { get; set; } = string.Empty;  // 內容
            public bool Enable { get; set; } = true;          // 是否可編輯
        }
    }
}
