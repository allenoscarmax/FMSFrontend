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
                    var customStyle = new Style(typeof(TabItem), baseStyle);

                    // 🔍 判斷是否來自子 TabControl（例如有 Tag = SubTab）
                    bool isSubTab = false;
                    DependencyObject parent = tabItem;
                    while (parent != null && !(parent is TabControl))
                        parent = VisualTreeHelper.GetParent(parent);

                    if (parent is TabControl tabControl &&
                        tabControl.Tag?.ToString() == "SubTab")
                    {
                        isSubTab = true;
                    }

                    // ✅ 預設底色（只有子Tab是灰色）
                    var defaultColor = isSubTab ? "#CCCCCC" : "#555555"; // 假設原主Tab是偏黑色
                    customStyle.Setters.Add(new Setter(Control.BackgroundProperty,
                        new SolidColorBrush((Color)ColorConverter.ConvertFromString(defaultColor))));

                    // ✅ 選中時使用 Tag 設定的顏色
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
