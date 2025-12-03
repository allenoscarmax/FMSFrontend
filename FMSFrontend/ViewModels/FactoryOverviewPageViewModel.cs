using FMSFrontend.Controls.FactoryOverview;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;
using FMSFrontend.Models;
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
        private readonly RobotStore _robotStore;

        public object FactoryLayoutContent { get; }
        public object TaskListContent { get; } = new(); // 右側先佔位

        public FactoryOverviewPageViewModel(ICommandScheduleService commandScheduleService,
            CommandScheduleStore commandScheduleStore,
            CommandScheduleLiveUpdater commandScheduleUpdater,
            RobotStore robotStore)
        {
            _commandScheduleService = commandScheduleService;
            CommandScheduleStore = commandScheduleStore;
            _commandScheduleLiveUpdater = commandScheduleUpdater;
            _robotStore = robotStore;

            var layout = new FactoryLayoutCanvasControl
            {
                DataContext = new FactoryLayoutViewModel(_robotStore)
            };
            var taskListControl = new TaskListControl(commandScheduleService, commandScheduleStore, commandScheduleUpdater);
            taskListControl.DataContext = new TaskListViewModel(commandScheduleService, commandScheduleStore, commandScheduleUpdater);

            FactoryLayoutContent = layout; //加入畫面
            TaskListContent = taskListControl; 
        }
    }
}
