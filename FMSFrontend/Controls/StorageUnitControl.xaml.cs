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

namespace FMSFrontend.Controls
{
    /// <summary>
    /// StorageUnitControl.xaml 的互動邏輯
    /// </summary>
    public partial class StorageUnitControl : UserControl
    {
        public StorageUnitControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty StorageTitleProperty =
            DependencyProperty.Register(nameof(StorageTitle), typeof(string), typeof(StorageUnitControl), new PropertyMetadata("未命名倉門"));

        public string StorageTitle
        {
            get => (string)GetValue(StorageTitleProperty);
            set => SetValue(StorageTitleProperty, value);
        }

        public static readonly DependencyProperty SidebarBrushProperty =
            DependencyProperty.Register(nameof(SidebarBrush), typeof(Brush), typeof(StorageUnitControl),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x27, 0x79, 0xA7))));

        public Brush SidebarBrush
        {
            get => (Brush)GetValue(SidebarBrushProperty);
            set => SetValue(SidebarBrushProperty, value);
        }

        public static readonly DependencyProperty StorageIdProperty =
    DependencyProperty.Register(
        nameof(StorageId),
        typeof(string),
        typeof(StorageUnitControl),
        new PropertyMetadata(default(string))
    );

        public string StorageId
        {
            get => (string)GetValue(StorageIdProperty);
            set => SetValue(StorageIdProperty, value);
        }

    }
}
