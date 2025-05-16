using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using FMSFrontend.Models;

namespace FMSFrontend.Controls
{
    /// <summary>
    /// RobotPositionPanel.xaml 的互動邏輯
    /// </summary>
    public partial class RobotPositionPanel : UserControl
    {
        public RobotPositionPanel()
        {
            InitializeComponent();

        }
        public Robot SelectedRobot
        {
            get => (Robot)GetValue(SelectedRobotProperty);
            set => SetValue(SelectedRobotProperty, value);
        }

        public static readonly DependencyProperty SelectedRobotProperty =
            DependencyProperty.Register(
                nameof(SelectedRobot),
                typeof(Robot),
                typeof(RobotPositionPanel),
                new PropertyMetadata(null));

    }
}

