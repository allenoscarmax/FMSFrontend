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
using System.Windows.Shapes;
using FMSFrontend.Features.Services; // 新增: 各種服務介面/實作
using FMSFrontend.Features.Singleton; // 新增: RFIDBindStore
using FMSFrontend.Features.Threading; // 新增: RFIDBindLiveUpdater

namespace FMSFrontend.Views.Windows
{
    /// <summary>
    /// ProbePairWindow.xaml 的互動邏輯
    /// </summary>
    public partial class ProbePairWindow : Window
    {
        public ProbePairWindow()
        {   
            InitializeComponent();
            var windowService = new WindowService();
            var httpService = new HttpService();

            // 建立必要服務 (需傳入 httpService)
            var electrodeService = new ElectrodeService(httpService);
            var probeService = new ProbeService(httpService);
            var rfidService = new RfidService(httpService);
            var workpieceService = new WorkpieceService(httpService);
            var worksheetsService = new WorksheetsService(httpService);

            // 建立資料存放與即時更新元件 (RFIDBindLiveUpdater 需 rfidService 與 store)
            var rfidBindStore = new RFIDBindStore();
            var rfidBindLiveUpdater = new RFIDBindLiveUpdater(rfidService, rfidBindStore);

            DataContext = new ProbePairViewModel(
                this,
                windowService,
                httpService,
                electrodeService,
                probeService,
                rfidService,
                workpieceService,
                worksheetsService,
                rfidBindStore,
                rfidBindLiveUpdater
            );
            Loaded += (_, __) => ((ProbePairViewModel)DataContext).OnPageActivated();
            Unloaded += (_, __) => ((ProbePairViewModel)DataContext).OnPageDeactivated();
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
