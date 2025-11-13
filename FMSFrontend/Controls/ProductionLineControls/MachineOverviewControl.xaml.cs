using FMSFrontend.ViewModels.Production;
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

namespace FMSFrontend.Controls
{
    /// <summary>
    /// MachineOverviewControl.xaml 的互動邏輯
    /// </summary>
    public partial class MachineOverviewControl : UserControl
    {
        public MachineOverviewControl()
        {
            InitializeComponent();
            Loaded += (_, __) => ((MachineOverviewViewModel)DataContext).OnPageActivated();
            Unloaded += (_, __) => ((MachineOverviewViewModel)DataContext).OnPageDeactivated();
        }
    }
}
