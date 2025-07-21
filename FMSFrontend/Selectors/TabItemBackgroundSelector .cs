using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FMSFrontend.Selectors
{
    public class TabItemBackgroundSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (container is TabItem tabItem)
            {
                var baseStyle = Application.Current.FindResource("RoundedTabItemBaseStyle") as Style;

                if (tabItem.Tag is string colorCode && !string.IsNullOrWhiteSpace(colorCode))
                {
                    // 建立一個新的樣式（基於 baseStyle）
                    var customStyle = new Style(typeof(TabItem), baseStyle);

                    // 加上 "IsSelected = True" 時才套用背景顏色
                    var trigger = new Trigger
                    {
                        Property = TabItem.IsSelectedProperty,
                        Value = true
                    };
                    trigger.Setters.Add(new Setter(Control.BackgroundProperty,
                        new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorCode))));

                    customStyle.Triggers.Add(trigger);
                    return customStyle;
                }

                return baseStyle;
            }

            return base.SelectStyle(item, container);
        }
    }

}
