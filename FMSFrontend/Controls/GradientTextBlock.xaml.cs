using System.Windows;
using System.Windows.Controls;

namespace FMSFrontend.Controls
{
    public partial class GradientTextBlock : UserControl
    {
        public GradientTextBlock()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(GradientTextBlock), new PropertyMetadata(""));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
    }
}
