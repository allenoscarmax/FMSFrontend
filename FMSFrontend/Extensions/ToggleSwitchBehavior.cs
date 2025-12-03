using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace FMSFrontend.Extensions
{
    public static class ToggleSwitchBehavior
    {
        public static readonly DependencyProperty EnableSmartAnimationProperty =
            DependencyProperty.RegisterAttached(
                "EnableSmartAnimation",
                typeof(bool),
                typeof(ToggleSwitchBehavior),
                new PropertyMetadata(false, OnEnableSmartAnimationChanged));

        public static void SetEnableSmartAnimation(DependencyObject obj, bool value)
            => obj.SetValue(EnableSmartAnimationProperty, value);

        public static bool GetEnableSmartAnimation(DependencyObject obj)
            => (bool)obj.GetValue(EnableSmartAnimationProperty);

        private static void OnEnableSmartAnimationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ToggleButton toggle) return;

            if ((bool)e.NewValue)
            {
                toggle.PreviewMouseDown += Toggle_PreviewMouseDown;
                toggle.Checked += Toggle_StateChanged;
                toggle.Unchecked += Toggle_StateChanged;
            }
            else
            {
                toggle.PreviewMouseDown -= Toggle_PreviewMouseDown;
                toggle.Checked -= Toggle_StateChanged;
                toggle.Unchecked -= Toggle_StateChanged;
            }
        }

        private static bool _isUserClick = false;

        private static void Toggle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isUserClick = true;
        }

        /// <summary>
        /// Checked / Unchecked 都會來到這裡
        /// </summary>
        private static void Toggle_StateChanged(object sender, RoutedEventArgs e)
        {
            if (sender is not ToggleButton toggle) return;

            string state = toggle.IsChecked == true ? "Checked" : "Unchecked";
            string instant = toggle.IsChecked == true ? "CheckedInstant" : "UncheckedInstant";

            if (_isUserClick)
            {
                // 使用者觸發 → 播放動畫
                VisualStateManager.GoToState(toggle, state, true);
            }
            else
            {
                // 程式改變（Binding）→ 無動畫
                VisualStateManager.GoToState(toggle, instant, false);
            }

            _isUserClick = false;
        }
    }
}
