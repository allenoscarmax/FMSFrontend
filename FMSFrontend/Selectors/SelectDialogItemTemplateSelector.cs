using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace FMSFrontend.Selectors
{
    public class SelectDialogItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? WorkOrderTemplate { get; set; }
        public DataTemplate? ElectrodeTemplate { get; set; }
        public DataTemplate? WorkpieceTemplate { get; set; }

        //public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        //{
        //    // 從 ItemsControl 的 DataContext 取得 VM 的 DialogType
        //    if (container is FrameworkElement fe && fe.DataContext is SelectItemWindowViewModel vm)
        //    {
        //        return vm.DialogType switch
        //        {
        //            SelectDialogType.WorkOrder => WorkOrderTemplate,
        //            SelectDialogType.Electrode => ElectrodeTemplate,
        //            SelectDialogType.Workpiece => WorkpieceTemplate,
        //            _ => base.SelectTemplate(item, container)
        //        };
        //    }
        //    return base.SelectTemplate(item, container);
        //}
    }
}
