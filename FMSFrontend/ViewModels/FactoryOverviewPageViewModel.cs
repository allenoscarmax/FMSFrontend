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
using System.Windows.Controls;

namespace FMSFrontend.ViewModels
{
    public class FactoryOverviewPageViewModel
    {
        
        private readonly ICommandScheduleService _commandScheduleService;
        // === Singleton ===
        private readonly CommandScheduleStore CommandScheduleStore;
        // ==LiveUpdater===
        public CommandScheduleLiveUpdater _commandScheduleLiveUpdater;
        
        public object FactoryLayoutContent { get; }
        public object TaskListContent { get; } = new(); // 右側先佔位

        public FactoryOverviewPageViewModel(IHttpService httpService, 
            CommandScheduleStore commandScheduleStore,
            CommandScheduleLiveUpdater commandScheduleUpdater)
        {
            _commandScheduleService = new CommandScheduleService(httpService);
            CommandScheduleStore = commandScheduleStore;
            _commandScheduleLiveUpdater = commandScheduleUpdater;

            var layout = new FactoryLayoutCanvasControl
            {
                DataContext = new FactoryLayoutViewModel(httpService, CommandScheduleStore)
            };
            var taskListControl = new TaskListControl(_commandScheduleService, commandScheduleStore);
            taskListControl.DataContext = new TaskListViewModel(_commandScheduleService, commandScheduleStore);

            FactoryLayoutContent = layout; //加入畫面
            TaskListContent = taskListControl; 
        }
        public void OnPageActivated()
        {
            _commandScheduleLiveUpdater.Start();
        }
        public void OnPageDeactivated()
        {
            _commandScheduleLiveUpdater.Stop();
        }
    }
}
