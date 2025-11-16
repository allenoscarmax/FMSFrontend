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
    public partial class MachineMainDetailControl : UserControl
    {
        public MachineMainDetailControl(MachineOverviewCard selectedMachine, IWorksheetsService worksheetsService, MachineStore machineStore)
        {
            InitializeComponent();

            this.DataContext = new MachineMainDetailViewModel(selectedMachine, worksheetsService, machineStore);
        }
        private void TabItem_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is TabItem tabItem && MachineInfoTabControl != null)
            {

                int index = MachineInfoTabControl.Items.IndexOf(tabItem);
                System.Diagnostics.Debug.WriteLine($"滑過第 {index} 項");
                if (index >= 0)
                {
                    MachineInfoTabControl.SelectedIndex = index;

                    // 若你有 ViewModel 的雙向綁定 SelectedIndex，也更新 ViewModel
                    if (DataContext is MachineMainDetailViewModel vm)
                        vm.MachineInfoTabControlSelectedIndex = index;
                }
            }
        }
    }
}
