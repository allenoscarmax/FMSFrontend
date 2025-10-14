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
    /// ElectrodeDetailView.xaml 的互動邏輯
    /// </summary>
    public partial class ElectrodeDetailView : UserControl
    {
        public ElectrodeDetailView()
        {
            InitializeComponent();
        }

        // IsLocked 切換鎖定狀態
        public static readonly DependencyProperty IsLockedProperty =
            DependencyProperty.Register(
                nameof(IsLocked),
                typeof(bool),
                typeof(ElectrodeDetailView),
                new PropertyMetadata(false));

        public bool IsLocked
        {
            get => (bool)GetValue(IsLockedProperty);
            set => SetValue(IsLockedProperty, value);
        }

        // 1 電極再使用（綠）
        public static readonly DependencyProperty ReuseElectrodeCommandProperty =
            DependencyProperty.Register(
                nameof(ReuseElectrodeCommand),
                typeof(ICommand),
                typeof(ElectrodeDetailView),
                new PropertyMetadata(null));

        public ICommand ReuseElectrodeCommand
        {
            get => (ICommand)GetValue(ReuseElectrodeCommandProperty);
            set => SetValue(ReuseElectrodeCommandProperty, value);
        }

        // 2 電極再使用（藍）
        public static readonly DependencyProperty ReuseElectrodeAltCommandProperty =
            DependencyProperty.Register(
                nameof(ReuseElectrodeAltCommand),
                typeof(ICommand),
                typeof(ElectrodeDetailView),
                new PropertyMetadata(null));

        public ICommand ReuseElectrodeAltCommand
        {
            get => (ICommand)GetValue(ReuseElectrodeAltCommandProperty);
            set => SetValue(ReuseElectrodeAltCommandProperty, value);
        }

        // 3 修改補償值（紫）
        public static readonly DependencyProperty EditCompensationCommandProperty =
            DependencyProperty.Register(
                nameof(EditCompensationCommand),
                typeof(ICommand),
                typeof(ElectrodeDetailView),
                new PropertyMetadata(null));

        public ICommand EditCompensationCommand
        {
            get => (ICommand)GetValue(EditCompensationCommandProperty);
            set => SetValue(EditCompensationCommandProperty, value);
        }

        // 4 解除異常（橘）
        public static readonly DependencyProperty ClearAbnormalCommandProperty =
            DependencyProperty.Register(
                nameof(ClearAbnormalCommand),
                typeof(ICommand),
                typeof(ElectrodeDetailView),
                new PropertyMetadata(null));

        public ICommand ClearAbnormalCommand
        {
            get => (ICommand)GetValue(ClearAbnormalCommandProperty);
            set => SetValue(ClearAbnormalCommandProperty, value);
        }

        // 5 解除碰撞（青藍）
        public static readonly DependencyProperty ClearCollisionCommandProperty =
            DependencyProperty.Register(
                nameof(ClearCollisionCommand),
                typeof(ICommand),
                typeof(ElectrodeDetailView),
                new PropertyMetadata(null));

        public ICommand ClearCollisionCommand
        {
            get => (ICommand)GetValue(ClearCollisionCommandProperty);
            set => SetValue(ClearCollisionCommandProperty, value);
        }

        // 6 解除預約（藍綠）
        public static readonly DependencyProperty ClearReservationCommandProperty =
            DependencyProperty.Register(
                nameof(ClearReservationCommand),
                typeof(ICommand),
                typeof(ElectrodeDetailView),
                new PropertyMetadata(null));

        public ICommand ClearReservationCommand
        {
            get => (ICommand)GetValue(ClearReservationCommandProperty);
            set => SetValue(ClearReservationCommandProperty, value);
        }
    }
}
