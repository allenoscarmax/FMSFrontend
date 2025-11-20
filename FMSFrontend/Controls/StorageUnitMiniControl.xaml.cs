using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

        #region 依舊的現有 DP
        public static readonly DependencyProperty StorageTitleProperty =
            DependencyProperty.Register(nameof(StorageTitle), typeof(string), typeof(StorageUnitMiniControl));

        public string StorageTitle
        {
            get => (string)GetValue(StorageTitleProperty);
            set => SetValue(StorageTitleProperty, value);
        }

        public static readonly DependencyProperty UpperDoorCommandProperty =
            DependencyProperty.Register(nameof(UpperDoorCommand), typeof(ICommand), typeof(StorageUnitMiniControl));

        public ICommand UpperDoorCommand
        {
            get => (ICommand)GetValue(UpperDoorCommandProperty);
            set => SetValue(UpperDoorCommandProperty, value);
        }

        public static readonly DependencyProperty LowerDoorCommandProperty =
            DependencyProperty.Register(nameof(LowerDoorCommand), typeof(ICommand), typeof(StorageUnitMiniControl));

        public ICommand LowerDoorCommand
        {
            get => (ICommand)GetValue(LowerDoorCommandProperty);
            set => SetValue(LowerDoorCommandProperty, value);
        }

        public static readonly DependencyProperty ShowDetailCommandProperty =
            DependencyProperty.Register(nameof(ShowDetailCommand), typeof(ICommand), typeof(StorageUnitMiniControl));

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
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x27, 0x79, 0xA7)))
            );

        public Brush SidebarBrush
        {
            get => (Brush)GetValue(SidebarBrushProperty);
            set => SetValue(SidebarBrushProperty, value);
        }
        #endregion

        #region 標題名稱的屬性 (Left/Right Title & Brush)
        public static readonly DependencyProperty LeftTitleBrushProperty =
            DependencyProperty.Register(nameof(LeftTitleBrush), typeof(Brush), typeof(StorageUnitMiniControl), new PropertyMetadata(Brushes.SteelBlue));

        public Brush LeftTitleBrush
        {
            get => (Brush)GetValue(LeftTitleBrushProperty);
            set => SetValue(LeftTitleBrushProperty, value);
        }

        public static readonly DependencyProperty LeftTitleProperty =
            DependencyProperty.Register(nameof(LeftTitle), typeof(string), typeof(StorageUnitMiniControl), new PropertyMetadata(string.Empty));

        public string LeftTitle
        {
            get => (string)GetValue(LeftTitleProperty);
            set => SetValue(LeftTitleProperty, value);
        }

        public static readonly DependencyProperty RightTitleBrushProperty =
            DependencyProperty.Register(nameof(RightTitleBrush), typeof(Brush), typeof(StorageUnitMiniControl), new PropertyMetadata(Brushes.DarkOrange));

        public Brush RightTitleBrush
        {
            get => (Brush)GetValue(RightTitleBrushProperty);
            set => SetValue(RightTitleBrushProperty, value);
        }

        public static readonly DependencyProperty RightTitleProperty =
            DependencyProperty.Register(nameof(RightTitle), typeof(string), typeof(StorageUnitMiniControl), new PropertyMetadata(string.Empty));

        public string RightTitle
        {
            get => (string)GetValue(RightTitleProperty);
            set => SetValue(RightTitleProperty, value);
        }
        #endregion

        #region 狀態燈對應的屬性 (Upper/Lower Scan & Door)
        public static readonly DependencyProperty UpperScanStatusProperty =
            DependencyProperty.Register(nameof(UpperScanStatus), typeof(Brush), typeof(StorageUnitMiniControl), new PropertyMetadata(Brushes.Gray));

        public Brush UpperScanStatus
        {
            get => (Brush)GetValue(UpperScanStatusProperty);
            set => SetValue(UpperScanStatusProperty, value);
        }

        public static readonly DependencyProperty UpperDoorStatusProperty =
            DependencyProperty.Register(nameof(UpperDoorStatus), typeof(Brush), typeof(StorageUnitMiniControl), new PropertyMetadata(Brushes.Gray));

        public Brush UpperDoorStatus
        {
            get => (Brush)GetValue(UpperDoorStatusProperty);
            set => SetValue(UpperDoorStatusProperty, value);
        }

        public static readonly DependencyProperty LowerScanStatusProperty =
            DependencyProperty.Register(nameof(LowerScanStatus), typeof(Brush), typeof(StorageUnitMiniControl), new PropertyMetadata(Brushes.Gray));

        public Brush LowerScanStatus
        {
            get => (Brush)GetValue(LowerScanStatusProperty);
            set => SetValue(LowerScanStatusProperty, value);
        }

        public static readonly DependencyProperty LowerDoorStatusProperty =
            DependencyProperty.Register(nameof(LowerDoorStatus), typeof(Brush), typeof(StorageUnitMiniControl), new PropertyMetadata(Brushes.Gray));

        public Brush LowerDoorStatus
        {
            get => (Brush)GetValue(LowerDoorStatusProperty);
            set => SetValue(LowerDoorStatusProperty, value);
        }
        #endregion
    }
}
