using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FMSFrontend.Views
{
    /// <summary>
    /// StorageUnitMiniControlPage.xaml 的互動邏輯
    /// </summary>
    public partial class StorageUnitMiniControlPage : UserControl
    {
        public StorageUnitMiniControlPage()
        {
            InitializeComponent();
        }

        public void ScrollToStorageId(string storageId)
        {
            if (storageId == null) return;
            foreach (var item in MagazineItems.Items)
            {
                if (item is Models.MagazineParaInfo info && info.StorageId == storageId)
                {
                    var container = MagazineItems.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                    container?.BringIntoView();
                    break;
                }
            }
        }
    }
}
