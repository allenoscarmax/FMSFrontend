using CommunityToolkit.Mvvm.Input;
using FMSFrontend.Extensions;
using OSCARMAXFMS_V3.DBmodels;
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

        #region DoorLight
        // 狀態燈 Brush（單一 Brush 即可）
        public static readonly DependencyProperty UpperDoorLight1Property =
            DependencyProperty.Register(
                nameof(UpperDoorLight1), 
                typeof(Brush), 
                typeof(StorageUnitMiniControl),
                new PropertyMetadata(Brushes.Gray)
            );

        public Brush UpperDoorLight1
        {
            get => (Brush)GetValue(UpperDoorLight1Property);
            set => SetValue(UpperDoorLight1Property, value);
        }

        public static readonly DependencyProperty UpperDoorLight2Property =
            DependencyProperty.Register(nameof(UpperDoorLight2), typeof(Brush), typeof(StorageUnitMiniControl),
                new PropertyMetadata(Brushes.Gray));

        public Brush UpperDoorLight2
        {
            get => (Brush)GetValue(UpperDoorLight2Property);
            set => SetValue(UpperDoorLight2Property, value);
        }

        public static readonly DependencyProperty LowerDoorLight1Property =
            DependencyProperty.Register(nameof(LowerDoorLight1), typeof(Brush), typeof(StorageUnitMiniControl),
                new PropertyMetadata(Brushes.Gray));

        public Brush LowerDoorLight1
        {
            get => (Brush)GetValue(LowerDoorLight1Property);
            set => SetValue(LowerDoorLight1Property, value);
        }

        public static readonly DependencyProperty LowerDoorLight2Property =
            DependencyProperty.Register(nameof(LowerDoorLight2), typeof(Brush), typeof(StorageUnitMiniControl),
                new PropertyMetadata(Brushes.Gray));

        public Brush LowerDoorLight2
        {
            get => (Brush)GetValue(LowerDoorLight2Property);
            set => SetValue(LowerDoorLight2Property, value);
        }

        #endregion
    }
}
