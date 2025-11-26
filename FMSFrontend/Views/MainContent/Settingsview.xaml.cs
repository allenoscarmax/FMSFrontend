using FMSFrontend.Services;
using FMSFrontend.ViewModels;
using FMSFrontend.ViewModels.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

namespace FMSFrontend.Views
{
    /// <summary>
    /// SettingsView.xaml 的互動邏輯
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView(SettingsPageViewModel viewModel)
        {
            InitializeComponent();

            this.DataContext = viewModel;

        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsPageViewModel vm && sender is PasswordBox pb)
            {
                vm.Password = pb.Password;
            }
        }

    }
}
