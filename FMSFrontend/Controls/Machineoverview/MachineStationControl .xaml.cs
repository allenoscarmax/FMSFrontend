using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.ViewModels;
using Microsoft.Extensions.DependencyInjection;
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
        public MachineStationControl(MachineOverviewCard selectedMachine,
                                MachineOverviewMainViewModel parent)
        {
            InitializeComponent();

            // 從 App 的靜態 ServiceProvider 取得 VM
            var vm = App.ServiceProvider!.GetRequiredService<MachineStationViewModel>();

            // 把「這一格要顯示哪台機」丟給 VM
            vm.Initialize(selectedMachine, parent);

            DataContext = vm;

            Loaded += (_, __) => vm.OnPageActivated();
            Unloaded += (_, __) => vm.OnPageDeactivated();
        }
    }
}
