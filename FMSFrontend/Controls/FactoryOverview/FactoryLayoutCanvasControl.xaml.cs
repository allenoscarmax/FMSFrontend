using FMSFrontend.ViewModels.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace FMSFrontend.Controls.FactoryOverview
{
    /// <summary>
    /// FactoryLayoutCanvasControl.xaml 的互動邏輯
    /// </summary>
    public partial class FactoryLayoutCanvasControl : UserControl
    {
        private const string DefaultJson = "layout.json";
        public FactoryLayoutCanvasControl()
        {
            InitializeComponent();
            // 👇 等畫面載入後訂閱 VM 事件
            this.Loaded += (_, __) =>
            {
                if (VM != null)
                {
                    // 避免重複訂閱，先解除一次（保險）
                    VM.RobotMoveRequested -= OnRobotMoveRequested;
                    VM.RobotMoveRequested += OnRobotMoveRequested;
                }
            };
        }
        private FactoryLayoutViewModel? VM => DataContext as FactoryLayoutViewModel;

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is not Thumb thumb || thumb.DataContext is not MachineNode node) return;

            node.X += e.HorizontalChange;
            node.Y += e.VerticalChange;

            var w = ActualWidth > 0 ? ActualWidth : 960;
            var h = ActualHeight > 0 ? ActualHeight : 640;

            node.X = Math.Max(0, Math.Min(w - node.Width, node.X));
            node.Y = Math.Max(0, Math.Min(h - node.Height, node.Y));

            const int grid = 10;
            node.X = Math.Round(node.X / grid) * grid;
            node.Y = Math.Round(node.Y / grid) * grid;
        }

        private void OnEditClick(object sender, RoutedEventArgs e)
        {
            if (VM != null) VM.IsEditMode = true;
        }

        private async void OnSaveClick(object sender, RoutedEventArgs e)
        {
            if (VM == null) return;
            try
            {
                await VM.SaveLayoutAsync(DefaultJson);
                VM.IsEditMode = false; // 儲存後退出編輯模式
                MessageBox.Show("已儲存並退出編輯模式\n" );
            }
            catch (Exception ex)
            {
                MessageBox.Show("儲存失敗：" + ex.Message);
            }
        }

        private async void OnLoadClick(object sender, RoutedEventArgs e)
        {
            if (VM == null) return;
            try
            {
                await VM.LoadLayoutAsync(DefaultJson);
            }
            catch (Exception ex)
            {
                MessageBox.Show("載入失敗：" + ex.Message);
            }
        }
        private void AnimateRobotTo(MachineNode robot, double newX, double newY)
        {
            // 找出對應的 UI 元素 (ContentPresenter)
            var container = (ContentPresenter)FactoryItemsControl.ItemContainerGenerator.ContainerFromItem(robot);
            if (container == null) return;

            // 對 Canvas.Left 動畫
            var animX = new DoubleAnimation
            {
                To = newX,
                Duration = TimeSpan.FromSeconds(0.8), // 移動速度
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(animX, container);
            Storyboard.SetTargetProperty(animX, new PropertyPath("(Canvas.Left)"));

            // 對 Canvas.Top 動畫
            var animY = new DoubleAnimation
            {
                To = newY,
                Duration = TimeSpan.FromSeconds(0.8),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(animY, container);
            Storyboard.SetTargetProperty(animY, new PropertyPath("(Canvas.Top)"));

            // 播放動畫
            var sb = new Storyboard();
            sb.Children.Add(animX);
            sb.Children.Add(animY);
            sb.Begin();
        }

        private void OnRobotMoveRequested(MachineNode robot, double x, double y)
        {
            // 在 UI 執行緒做動畫
            Dispatcher.Invoke(() =>
            {
                // 若找不到容器（例如剛生成），退而直接設值避免不動
                var container = (ContentPresenter?)FactoryItemsControl.ItemContainerGenerator.ContainerFromItem(robot);
                if (container == null)
                {
                    robot.X = x; robot.Y = y;
                    return;
                }

                AnimateRobotTo(robot, x, y);
            });
        }
    }
}
