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
    /// StorageUnitMiniControl.xaml 的互動邏輯
    /// </summary>
    public partial class StorageUnitMiniControl : UserControl
    {
        public StorageUnitMiniControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty StorageTitleProperty =
    DependencyProperty.Register("StorageTitle", typeof(string), typeof(StorageUnitMiniControl));

        public string StorageTitle
        {
            get => (string)GetValue(StorageTitleProperty);
            set => SetValue(StorageTitleProperty, value);
        }

        public static readonly DependencyProperty UpperDoorCommandProperty =
            DependencyProperty.Register("UpperDoorCommand", typeof(ICommand), typeof(StorageUnitMiniControl));

        public ICommand UpperDoorCommand
        {
            get => (ICommand)GetValue(UpperDoorCommandProperty);
            set => SetValue(UpperDoorCommandProperty, value);
        }

        public static readonly DependencyProperty LowerDoorCommandProperty =
            DependencyProperty.Register("LowerDoorCommand", typeof(ICommand), typeof(StorageUnitMiniControl));

        public ICommand LowerDoorCommand
        {
            get => (ICommand)GetValue(LowerDoorCommandProperty);
            set => SetValue(LowerDoorCommandProperty, value);
        }

        public static readonly DependencyProperty ShowDetailCommandProperty =
    DependencyProperty.Register("ShowDetailCommand", typeof(ICommand), typeof(StorageUnitMiniControl));

        public ICommand ShowDetailCommand
        {
            get => (ICommand)GetValue(ShowDetailCommandProperty);
            set => SetValue(ShowDetailCommandProperty, value);
        }

        public static readonly DependencyProperty StorageIdProperty =
    DependencyProperty.Register(nameof(StorageId), typeof(string), typeof(StorageUnitMiniControl), new PropertyMetadata(""));

        public string StorageId
        {
            get => (string)GetValue(StorageIdProperty);
            set => SetValue(StorageIdProperty, value);
        }

        public static readonly DependencyProperty SidebarBrushProperty =
    DependencyProperty.Register(
        nameof(SidebarBrush),
        typeof(Brush),
        typeof(StorageUnitMiniControl),
        new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x27, 0x79, 0xA7)))  // 預設為藍色
    );

        public Brush SidebarBrush
        {
            get => (Brush)GetValue(SidebarBrushProperty);
            set => SetValue(SidebarBrushProperty, value);
        }


    }
}
