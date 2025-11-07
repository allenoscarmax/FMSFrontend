using FMSFrontend.Services;
using FMSFrontend.ViewModels.Windows;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Features.Threading;

namespace FMSFrontend.Extensions
{
    /// <summary>
    /// MaterialPairWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MaterialPairWindow : Window
    {
        public MaterialPairWindow(bool isElectrode)
        {
            var windowService = new WindowService();
            var httpService = new HttpService();

            var electrodeService = new ElectrodeService(httpService);
            var probeService = new ProbeService(httpService);
            var rfidService = new RfidService(httpService);
            var workpieceService = new WorkpieceService(httpService);
            var worksheetsService = new WorksheetsService(httpService);

            var rfidBindStore = new RFIDBindStore();
            var rfidLiveUpdater = new RFIDBindLiveUpdater(rfidService, rfidBindStore);
            rfidLiveUpdater.Start();

            InitializeComponent();
            DataContext = new MaterialPairViewModel(
                isElectrode,
                this,
                windowService,
                httpService,
                electrodeService,
                probeService,
                rfidService,
                workpieceService,
                worksheetsService,
                rfidBindStore,
                rfidLiveUpdater);
            Loaded += (_, __) => ((MaterialPairViewModel)DataContext).OnPageActivated();
            Unloaded += (_, __) => ((MaterialPairViewModel)DataContext).OnPageDeactivated();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) DragMove();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
        }
    }
}
