using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace FMSFrontend.Controls
{
    public partial class PageMenuItem : UserControl
    {
        public PageMenuItem()
        {
            InitializeComponent();

        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(string), typeof(PageMenuItem));

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(PageMenuItem));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(PageMenuItem));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(PageMenuItem));

        public static readonly DependencyProperty PageKeyProperty =
            DependencyProperty.Register(nameof(PageKey), typeof(string), typeof(PageMenuItem),
                new PropertyMetadata(null, OnPageKeyChanged));

        public static readonly DependencyProperty CurrentPageKeyProperty =
            DependencyProperty.Register(nameof(CurrentPageKey), typeof(string), typeof(PageMenuItem),
                new PropertyMetadata(null, OnCurrentPageKeyChanged));

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(PageMenuItem), new PropertyMetadata(false));

        public static readonly DependencyProperty IsPreviousToSelectedProperty =
            DependencyProperty.Register(nameof(IsPreviousToSelected), typeof(bool), typeof(PageMenuItem), new PropertyMetadata(false));

        public static readonly DependencyProperty IsNextToSelectedProperty =
            DependencyProperty.Register(nameof(IsNextToSelected), typeof(bool), typeof(PageMenuItem), new PropertyMetadata(false));

        public static readonly DependencyProperty CornerRadiusProperty =
    DependencyProperty.Register(
        nameof(CornerRadius),
        typeof(CornerRadius),
        typeof(PageMenuItem),
        new FrameworkPropertyMetadata(
            new CornerRadius(30),
            FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty SelectedBackgroundProperty =
    DependencyProperty.Register(
        nameof(SelectedBackground),
        typeof(Brush),
        typeof(PageMenuItem),
        new FrameworkPropertyMetadata(
            Brushes.Transparent,
            FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty OverlayBackgroundProperty =
    DependencyProperty.Register(
        nameof(OverlayBackground),
        typeof(Brush),
        typeof(PageMenuItem),
        new FrameworkPropertyMetadata(
            Brushes.Transparent,
            FrameworkPropertyMetadataOptions.AffectsRender));

        public static new readonly DependencyProperty MarginProperty = DependencyProperty.Register(
            nameof(Margin),
            typeof(Thickness),
            typeof(PageMenuItem),
            new FrameworkPropertyMetadata(new Thickness(0), FrameworkPropertyMetadataOptions.AffectsRender));



        public static readonly DependencyProperty SelectedBackgroundKeyProperty =
    DependencyProperty.Register(
        nameof(SelectedBackgroundKey),
        typeof(string),
        typeof(PageMenuItem),
        new PropertyMetadata("PageMenuBarBrush"));

        public string SelectedBackgroundKey
        {
            get => (string)GetValue(SelectedBackgroundKeyProperty);
            set => SetValue(SelectedBackgroundKeyProperty, value);
        }

        public static readonly DependencyProperty OverlayBackgroundKeyProperty =
    DependencyProperty.Register(
        nameof(OverlayBackgroundKey),
        typeof(string),
        typeof(PageMenuItem),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public string OverlayBackgroundKey
        {
            get => (string)GetValue(OverlayBackgroundKeyProperty);
            set => SetValue(OverlayBackgroundKeyProperty, value);
        }


        public Brush OverlayBackground
        {
            get => (Brush)GetValue(OverlayBackgroundProperty);
            set => SetValue(OverlayBackgroundProperty, value);
        }


        public Brush SelectedBackground
        {
            get => (Brush)GetValue(SelectedBackgroundProperty);
            set => SetValue(SelectedBackgroundProperty, value);
        }

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

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public bool IsPreviousToSelected
        {
            get => (bool)GetValue(IsPreviousToSelectedProperty);
            set => SetValue(IsPreviousToSelectedProperty, value);
        }

        public bool IsNextToSelected
        {
            get => (bool)GetValue(IsNextToSelectedProperty);
            set => SetValue(IsNextToSelectedProperty, value);
        }

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        private static void OnCurrentPageKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PageMenuItem item)
            {
                item.IsSelected = item.PageKey == item.CurrentPageKey;
                item.UpdateCornerRadius();
            }
        }

        private static void OnPageKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PageMenuItem item)
            {
                item.IsSelected = item.PageKey == item.CurrentPageKey;
                item.UpdateCornerRadius();
            }
        }


        public new Thickness Margin
        {
            get => (Thickness)GetValue(MarginProperty);
            set => SetValue(MarginProperty, value);
        }

        public void UpdateCornerRadius()
        {
            var pageBrush = "PageViewBrush";
            var pageBrush_original = "PageMenuBarBrush";

            if (IsSelected)
            {
                CornerRadius = new CornerRadius(0);
                SelectedBackgroundKey = pageBrush;
                OverlayBackgroundKey = String.Empty;
                Margin = new Thickness(0);
            }
            else if (IsPreviousToSelected && IsFirstItem()) // 處理第一個項的左圓角
            {
                CornerRadius = new CornerRadius(30, 30, 0, 0);
                SelectedBackgroundKey = pageBrush_original;
                OverlayBackgroundKey = pageBrush;
                Margin = new Thickness(0, 0, -1, 0);
            }
            else if (IsPreviousToSelected && !IsFirstItem()) // 處理第一個項的左圓角
            {
                CornerRadius = new CornerRadius(0, 30, 0, 0);
                SelectedBackgroundKey = pageBrush_original;
                OverlayBackgroundKey = pageBrush;
                Margin = new Thickness(0, 0, -1, 0);
            }
            else if (!IsPreviousToSelected && IsFirstItem()) // 處理第一個項的左圓角
            {
                CornerRadius = new CornerRadius(30, 0, 0, 0);
                SelectedBackgroundKey = pageBrush_original;
                OverlayBackgroundKey = pageBrush;
                Margin = new Thickness(0, 0, -1, 0);
            }
            else if (IsNextToSelected || (IsSelected && IsLastItem())) // 處理最後一個項的右圓角
            {
                CornerRadius = new CornerRadius(30, 0, 0, 0);
                SelectedBackgroundKey = pageBrush_original;
                OverlayBackgroundKey = pageBrush;
                Margin = new Thickness(-1, 0, 0, 0);
            }
            else
            {
                CornerRadius = new CornerRadius(0);
                SelectedBackgroundKey = String.Empty;
                OverlayBackgroundKey = String.Empty;
                Margin = new Thickness(0);
            }
        }
        private bool IsFirstItem()
        {
            if (this.Parent is Panel panel)
            {
                var items = panel.Children.OfType<PageMenuItem>().ToList();
                return items.FirstOrDefault() == this;
            }
            return false;
        }

        private bool IsLastItem()
        {
            if (this.Parent is Panel panel)
            {
                var items = panel.Children.OfType<PageMenuItem>().ToList();
                return items.LastOrDefault() == this;
            }
            return false;
        }

    }
}
