using FMSFrontend.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace FMSFrontend.Views.Windows
{
    /// <summary>
    /// ReviseProcessWindow.xaml 的互動邏輯
    /// </summary>
    public partial class ReviseProcessWindow : Window
    {
        public ReviseProcessWindow(ReviseProcessViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();

            vm.PropertyChanged += Vm_PropertyChanged;
        }
        private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(ReviseProcessViewModel.SelectedAction))
                return;

            if (DataContext is ReviseProcessViewModel vm &&
                vm.SelectedAction.HasValue)
            {
                DialogResult = vm.SelectedAction != ReviseProcessAction.Cancel;
                Close();
            }
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
