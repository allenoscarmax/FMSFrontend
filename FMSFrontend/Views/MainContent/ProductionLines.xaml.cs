using FMSFrontend.Services;
using FMSFrontend.ViewModels;
using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace FMSFrontend.Views
{
    /// <summary>
    /// ProductionLines.xaml 的互動邏輯
    /// </summary>
    public partial class ProductionLines : UserControl
    {
        public ProductionLines(ProductionLinesViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            Loaded += (_, __) => ((ProductionLinesViewModel)DataContext).OnPageActivated();
            Unloaded += (_, __) => ((ProductionLinesViewModel)DataContext).OnPageDeactivated();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
            if (this.Resources["MoveBorderStoryboard"] is Storyboard sb)
            {
                sb.Begin(this, true);
            }
            
        }

        private void MovingBorder_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (this.Resources["MoveBorderStoryboard"] is Storyboard sb)
            {
                sb.Pause(this);
            }
        }

        private void MovingBorder_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (this.Resources["MoveBorderStoryboard"] is Storyboard sb)
            {
                sb.Resume(this);
            }
        }
    }
}
