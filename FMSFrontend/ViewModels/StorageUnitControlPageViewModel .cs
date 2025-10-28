using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Extensions;
using FMSFrontend.Services;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows;
using FMSFrontend.Helpers;
using System.Windows.Controls; // 放在你的 ViewModel 上方
using FMSFrontend.Views;

namespace FMSFrontend.ViewModels
{
    public class StorageUnitControlPageViewModel
    {
        public string ScrollToStorageId { get; set; } = "";
        // 其他屬性和方法
    }
}
