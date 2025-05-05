// PageMenuItem.xaml.cs
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FMSFrontend.Controls
{
    public partial class PageMenuItem : UserControl
    {
        public PageMenuItem() => InitializeComponent();

        public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(string), typeof(PageMenuItem));

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(PageMenuItem));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(PageMenuItem));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(PageMenuItem));

        public static readonly DependencyProperty PageKeyProperty =
    DependencyProperty.Register(nameof(PageKey), typeof(string), typeof(PageMenuItem));

        public static readonly DependencyProperty CurrentPageKeyProperty =
            DependencyProperty.Register(nameof(CurrentPageKey), typeof(string), typeof(PageMenuItem),
                new PropertyMetadata(null, OnCurrentPageKeyChanged));

        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }
        public string PageKey
        {
            get => (string)GetValue(PageKeyProperty);
            set => SetValue(PageKeyProperty, value);
        }

        public string CurrentPageKey
        {
            get => (string)GetValue(CurrentPageKeyProperty);
            set => SetValue(CurrentPageKeyProperty, value);
        }
        public bool IsSelected => PageKey == CurrentPageKey;

        private static void OnCurrentPageKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PageMenuItem item)
            {
                item.UpdateVisualState();
            }
        }
        private void UpdateVisualState()
        {
            var border = (Border)this.Template?.FindName("PART_Border", this);
            if (border != null)
            {
                border.Background = IsSelected
                    ? new SolidColorBrush(Color.FromRgb(68, 68, 68))  // 選中的底色
                    : Brushes.Transparent;
            }
        }



    }
}