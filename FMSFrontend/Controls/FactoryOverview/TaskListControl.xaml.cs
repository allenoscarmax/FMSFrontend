using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;
using FMSFrontend.ViewModels;
using FMSFrontend.ViewModels.Factory;
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

namespace FMSFrontend.Controls.FactoryOverview
{
    /// <summary>
    /// TaskListControl.xaml 的互動邏輯
    /// </summary>
    public partial class TaskListControl : UserControl
    {
        public TaskListControl(ICommandScheduleService commandScheduleService,
            CommandScheduleStore commandScheduleStore,
            CommandScheduleLiveUpdater commandScheduleLiveUpdater) 
        {
            InitializeComponent();
            // 假設有方法可以取得這三個必要參數，請根據實際情況替換

            DataContext = new TaskListViewModel(commandScheduleService, commandScheduleStore, commandScheduleLiveUpdater);
            Loaded += (_, __) => ((TaskListViewModel)DataContext).OnPageActivated();
            Unloaded += (_, __) => ((TaskListViewModel)DataContext).OnPageDeactivated();
        }

    }
}
