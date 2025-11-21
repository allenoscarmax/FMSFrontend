using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.ViewModels;
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
    /// MachineMainDetailControl.xaml 的互動邏輯
    /// </summary>
    public partial class MachineStationControl : UserControl
    {
        public MachineStationControl(MachineOverviewCard selectedMachine, MachineOverviewMainViewModel _parent )
        {
            InitializeComponent();
            this.DataContext = new MachineStationViewModel(selectedMachine, _parent);
            Loaded += (_, __) => ((MachineStationViewModel)DataContext).OnPageActivated();
            Unloaded += (_, __) => ((MachineStationViewModel)DataContext).OnPageDeactivated();
        }
    }
}
