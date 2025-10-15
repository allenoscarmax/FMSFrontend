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
using System.Collections.ObjectModel;

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
            DependencyProperty.Register(nameof(StorageId), typeof(string), typeof(StorageUnitControl), new PropertyMetadata(string.Empty));

        public string StorageId
        {
            get => (string)GetValue(StorageIdProperty);
            set => SetValue(StorageIdProperty, value);
        }

        public static readonly DependencyProperty LeftTitleBrushProperty =
            DependencyProperty.Register(nameof(LeftTitleBrush), typeof(Brush), typeof(StorageUnitControl), new PropertyMetadata(Brushes.SteelBlue));

        public Brush LeftTitleBrush
        {
            get => (Brush)GetValue(LeftTitleBrushProperty);
            set => SetValue(LeftTitleBrushProperty, value);
        }

        public static readonly DependencyProperty LeftTitleProperty =
            DependencyProperty.Register(nameof(LeftTitle), typeof(string), typeof(StorageUnitControl), new PropertyMetadata(string.Empty));

        public string LeftTitle
        {
            get => (string)GetValue(LeftTitleProperty);
            set => SetValue(LeftTitleProperty, value);
        }

        public static readonly DependencyProperty RightTitleBrushProperty =
            DependencyProperty.Register(nameof(RightTitleBrush), typeof(Brush), typeof(StorageUnitControl), new PropertyMetadata(Brushes.DarkOrange));

        public Brush RightTitleBrush
        {
            get => (Brush)GetValue(RightTitleBrushProperty);
            set => SetValue(RightTitleBrushProperty, value);
        }

        public static readonly DependencyProperty RightTitleProperty =
            DependencyProperty.Register(nameof(RightTitle), typeof(string), typeof(StorageUnitControl), new PropertyMetadata(string.Empty));

        public string RightTitle
        {
            get => (string)GetValue(RightTitleProperty);
            set => SetValue(RightTitleProperty, value);
        }

        //上門按鈕
        public static readonly DependencyProperty UpperDoorCommandProperty =
            DependencyProperty.Register(nameof(UpperDoorCommand), typeof(ICommand), typeof(StorageUnitControl), new PropertyMetadata(null));

        public ICommand UpperDoorCommand
        {
            get => (ICommand)GetValue(UpperDoorCommandProperty);
            set => SetValue(UpperDoorCommandProperty, value);
        }

        //下門按鈕
        public static readonly DependencyProperty LowerDoorCommandProperty =
            DependencyProperty.Register(nameof(LowerDoorCommand), typeof(ICommand), typeof(StorageUnitControl), new PropertyMetadata(null));

        public ICommand LowerDoorCommand
        {
            get => (ICommand)GetValue(LowerDoorCommandProperty);
            set => SetValue(LowerDoorCommandProperty, value);
        }
        #region DoorLight
        // 狀態燈 Brush（單一 Brush 即可）
        public static readonly DependencyProperty UpperDoorLight1Property =
            DependencyProperty.Register(
                nameof(UpperDoorLight1),
                typeof(Brush),
                typeof(StorageUnitControl),
                new PropertyMetadata(Brushes.Gray)
            );

        public Brush UpperDoorLight1
        {
            get => (Brush)GetValue(UpperDoorLight1Property);
            set => SetValue(UpperDoorLight1Property, value);
        }

        public static readonly DependencyProperty UpperDoorLight2Property =
            DependencyProperty.Register(nameof(UpperDoorLight2), typeof(Brush), typeof(StorageUnitControl),
                new PropertyMetadata(Brushes.Gray));

        public Brush UpperDoorLight2
        {
            get => (Brush)GetValue(UpperDoorLight2Property);
            set => SetValue(UpperDoorLight2Property, value);
        }

        public static readonly DependencyProperty LowerDoorLight1Property =
            DependencyProperty.Register(nameof(LowerDoorLight1), typeof(Brush), typeof(StorageUnitControl),
                new PropertyMetadata(Brushes.Gray));

        public Brush LowerDoorLight1
        {
            get => (Brush)GetValue(LowerDoorLight1Property);
            set => SetValue(LowerDoorLight1Property, value);
        }

        public static readonly DependencyProperty LowerDoorLight2Property =
            DependencyProperty.Register(nameof(LowerDoorLight2), typeof(Brush), typeof(StorageUnitControl),
                new PropertyMetadata(Brushes.Gray));

        public Brush LowerDoorLight2
        {
            get => (Brush)GetValue(LowerDoorLight2Property);
            set => SetValue(LowerDoorLight2Property, value);
        }

        #endregion
    }
}
