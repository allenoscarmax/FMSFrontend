using FMSFrontend.Controls.FactoryOverview;
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
        public object FactoryLayoutContent { get; }
        public object TaskListContent { get; } = new ContentControl(); // 右側先佔位

        public FactoryOverviewPageViewModel()
        {
            var layout = new FactoryLayoutCanvasControl
            {
                DataContext = new FactoryLayoutViewModel()
            };
            var Task = new TaskListControl
            {
                DataContext = new TaskListViewModel()
            };
            FactoryLayoutContent = layout;
            TaskListContent = Task; 
        }
    }
}
