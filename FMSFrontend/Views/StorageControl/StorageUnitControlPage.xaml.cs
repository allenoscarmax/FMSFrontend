using FMSFrontend.Controls;
using System.Windows.Controls;
using System.Windows; // for FrameworkElement

namespace FMSFrontend.Views
{
    /// <summary>
    /// StorageUnitControlPage.xaml 的互動邏輯
    /// </summary>
    public partial class StorageUnitControlPage : UserControl
    {
        public StorageUnitControlPage()
        {
            InitializeComponent();
        }

        public void ScrollToStorageId(string storageId)
        {
            // 透過 ItemsControl 的 Items 遍歷資料項，找到 StorageId 相符者並 BringIntoView
            if (storageId == null) return;
            foreach (var item in MagazineItems.Items)
            {
                if (item is Models.MagazineParaInfo info && info.StorageId == storageId)
                {
                    // 找出對應生成的容器元素
                    var container = MagazineItems.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                    container?.BringIntoView();
                    break;
                }
            }
        }
    }
}
