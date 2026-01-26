using FMSFrontend.Controls.FactoryOverview;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;
using FMSFrontend.Models;
using FMSFrontend.Services;
using FMSFrontend.ViewModels.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FMSFrontend.ViewModels
{
    public class FactoryOverviewPageViewModel
    {
        // ==LiveUpdater===
        public CommandScheduleLiveUpdater _commandScheduleLiveUpdater;

        public object FactoryLayoutContent { get; }
        public object TaskListContent { get; } = new(); // 右側先佔位

        public FactoryOverviewPageViewModel(
            CommandScheduleLiveUpdater commandScheduleLiveUpdater,
            FactoryLayoutCanvasControl layoutControl,
            FactoryLayoutViewModel factoryLayoutViewModel,
            TaskListControl taskListControl,
            TaskListViewModel taskListViewModel)
        {
            _commandScheduleLiveUpdater = commandScheduleLiveUpdater;
            layoutControl.DataContext = factoryLayoutViewModel;
            taskListControl.DataContext = taskListViewModel;
            FactoryLayoutContent = layoutControl;
            TaskListContent = taskListControl;
        }
        public void OnPageActivated()
        {
            _commandScheduleLiveUpdater.Start();
        }
        public void OnPageDeactivated()
        {
            _commandScheduleLiveUpdater.Stop();
            // 避免 Store 訂閱造成 memory leak：Page 關閉時把子 VM Dispose 掉
            if (FactoryLayoutContent is FrameworkElement fe1 && fe1.DataContext is IDisposable d1) 
                d1.Dispose();
            if (TaskListContent is FrameworkElement fe2 && fe2.DataContext is IDisposable d2) 
                d2.Dispose();
        }
    }
}
