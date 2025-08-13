using FMSFrontend.ViewModels.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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

namespace FMSFrontend.Extensions
{
    /// <summary>
    /// MaterialTypeSelectWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MaterialTypeSelectWindow : Window
    {
        public bool IsElectrodeSelected { get; private set; }
        public MaterialTypeSelectWindow()
        {
            InitializeComponent();
           
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void SelectElectrodeCommand(object sender, RoutedEventArgs e)
        {
            IsElectrodeSelected = true;
            DialogResult = true;
            Close();
        }

        private void SelectWorkpieceCommand(object sender, RoutedEventArgs e)
        {
            IsElectrodeSelected = false;
            DialogResult = true;
            Close();
        }
    }

}
