using FMSFrontend.Extensions;
using FMSFrontend.Features.Services;
using FMSFrontend.Services;
using FMSFrontend.ViewModels.Windows;
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

namespace FMSFrontend.Extensions
{
    /// <summary>
    /// SelectSharedElectrodeWindow.xaml 的互動邏輯
    /// </summary>
    public partial class SelectSharedElectrodeWindow : Window
    {
        string _targetWorkpieceName; // Share 時傳入的工件名稱
        public SelectSharedElectrodeWindow(string targetWorkpieceName)
        {
            var httpService = new HttpService();
            var windowService = new WindowService();
            var electrodeService = new ElectrodeService(httpService);
            var worksheetsService = new WorksheetsService(httpService);
            _targetWorkpieceName = targetWorkpieceName;
        InitializeComponent();
            var vm = new SelectSharedElectrodeViewModel(worksheetsService, electrodeService, Owner, _targetWorkpieceName);
            DataContext = vm;


            // 訂閱 ViewModel 的關閉事件
            this.Loaded += (s, e) =>
            {
                if (DataContext is SelectSharedElectrodeViewModel vm)
                {
                    vm.CloseRequested += (sender, result) =>
                    {
                        // 若你要回傳資料到呼叫端，可以放在 Tag 或 DialogResult
                        this.Tag = result;

                        // 關閉視窗
                        this.DialogResult = true;
                        this.Close();
                    };
                }
            };
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }

}
