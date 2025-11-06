using ControlzEx.Standard;
using FMSFrontend.Extensions;
using FMSFrontend.Interfaces;
using FMSFrontend.ViewModels;
using FMSFrontend.ViewModels.Windows;
using FMSFrontend.Views.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static FMSFrontend.ViewModels.ElectrodeDetailViewModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages; 

namespace FMSFrontend.Services
{
    public class WindowService : IWindowService
    {
        private ShowMaterialWindow? _materialWindow;
        // 移除 readonly，改為可為 null 的欄位，稍後在 EnsureMaterialWindow 建立
        private ShowMaterialWindowViewModel? _vm;  // ← 單一 VM，重複使用

        public void ShowUploadSheetWindow()
        {
            var window = new UploadsheetsWindow();
            window.ShowDialog();
            // 新增：上傳視窗關閉後廣播訊息，讓其他 ViewModel 可接收到並刷新資料
            WeakReferenceMessenger.Default.Send(new UploadSheetsClosedMessage(true));

        }

        public void ShowMessage(string message)
        {
            var dialog = new DialogMessageWindow(message);
            dialog.ShowDialog();
        }

        public bool ShowYesNoDialog(string message)
        {
            var dialog = new DialogYesNoWindow(message);
            return dialog.ShowDialog() == true;
        }
        public bool ShowMaterialTypeSelectWindow(out MaterialKind Kind)
        {
            var window = new MaterialTypeSelectWindow();
            bool? retu = window.ShowDialog();
            Kind = window.materialKind;


            return retu == true;
        }
        public void ShowMaterialPairWindow(bool isElectrode)
        {

            var window = new MaterialPairWindow(isElectrode); // 讓 ViewModel 在 window 裡綁定
            window.ShowDialog();
        }

        //public void ShowMaterialEmpty()
        //{
        //    var vm = new ShowMaterialWindowViewModel(); // 走空建構子
        //    var win = new ShowMaterialWindow(vm)
        //    {
        //        Owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
        //    };
        //    win.ShowDialog();
        //}

        // ★ 通用：detail VM + timeline + 類別
        public void ShowMaterial(object detailViewModel, IEnumerable<TimelineItemViewModel> timeline, MaterialKind kind, IHttpService httpService, string? slotCode = null)
        {
            EnsureMaterialWindow(httpService);
            _vm!.Kind = kind;
            _vm.SlotCode = slotCode;
            _vm.DetailViewModel = detailViewModel;

            _vm.Timeline.Clear();
            if (timeline != null)
            {
                foreach (var t in timeline)
                    _vm.Timeline.Add(t);
            }

            ShowOrActivate();
        }

        public void ShowWorkpiece(WorkpieceModel workpiece, IEnumerable<TimelineItemModel> timeline, IHttpService httpService, string? slotCode = null)
        {
            EnsureMaterialWindow(httpService);
            _vm!.Kind = MaterialKind.Workpiece;
            _vm.SlotCode = slotCode ?? (!string.IsNullOrWhiteSpace(workpiece?.No) ? workpiece.No : workpiece?.Name);
            _vm.DetailViewModel = new WorkpieceDetailViewModel(workpiece ?? new WorkpieceModel());

            FillTimeline(timeline);               // ← 把 timeline 塞回去
            ShowOrActivate();
        }

        public void ShowElectrode(ElectrodeModel electrode, IEnumerable<TimelineItemModel> timeline, IHttpService httpService, string? slotCode = null)
        {
            EnsureMaterialWindow(httpService);
            _vm!.Kind = MaterialKind.Electrode;
            _vm.SlotCode = slotCode ?? (!string.IsNullOrWhiteSpace(electrode?.No) ? electrode.No : electrode?.Name);
            _vm.DetailViewModel = new ElectrodeDetailViewModel(electrode?? new ElectrodeModel());

            FillTimeline(timeline);               // ← 把 timeline 塞回去
            ShowOrActivate();
        }

        public void ShowMaterialEmpty( IHttpService httpService)
        {
            EnsureMaterialWindow(httpService);
            _vm!.Kind = MaterialKind.None;
            _vm.SlotCode = null;
            _vm.DetailViewModel = new EmptyMaterialDetailViewModel();
            _vm.Timeline.Clear();
            ShowOrActivate();
        }


        private void EnsureMaterialWindow(IHttpService httpService)
        {
            if (_materialWindow == null)
            {
                _vm = new ShowMaterialWindowViewModel(httpService);
                _materialWindow = new ShowMaterialWindow(_vm)   // ← 傳入 vm
                {
                    Owner = Application.Current.MainWindow
                };
                _materialWindow.Closed += (_, __) => _materialWindow = null;
            }
        }

        private void ShowOrActivate()
        {
            if (_materialWindow!.IsVisible) _materialWindow.Activate();
            else _materialWindow.Show();
        }
        private void FillTimeline(IEnumerable<TimelineItemModel> timeline)
        {
            _vm!.Timeline.Clear();
            if (timeline == null) return;

            foreach (var t in timeline)
            {
                // 你的 VM 現在是 ObservableCollection<TimelineItemViewModel>
                _vm.Timeline.Add(new TimelineItemViewModel(t));
                // 如果你改成用 Model：_vm.Timeline.Add(t);
            }
        }
        private static void FillTimeline(IEnumerable<TimelineItemModel> timeline, ShowMaterialWindowViewModel targetVm)
        {
            if (targetVm == null) return;
            var dst = targetVm.Timeline; // 這應該是 ObservableCollection<TimelineItemViewModel>
            if (dst == null) return;

            dst.Clear();
            if (timeline == null) return;

            foreach (var it in timeline)
            {
                // 這裡做 Model -> ViewModel 的轉換
                dst.Add(new TimelineItemViewModel(it));
            }
        }


        // ===== 資訊版視窗的欄位 =====
        private ShowMaterialInformationWindow? _infoWindow;
        private ShowMaterialWindowViewModel? _infoVm;

        // ===== 你要的公開 API：Electrode =====
        public void ShowMaterialInformation(ElectrodeModel electrode, IEnumerable<TimelineItemModel> timeline,IHttpService httpService)
        {
            EnsureMaterialInformationWindow(httpService);

            _infoVm!.Kind = MaterialKind.Electrode;
            _infoVm.SlotCode = null; // 資訊視窗不顯示倉位/操作列
            _infoVm.DetailViewModel = new ElectrodeDetailViewModel(electrode);

            FillTimeline(timeline, _infoVm);
            ShowOrActivateInformation();
        }

        // ===== 你要的公開 API：Workpiece（補齊介面需求） =====
        public void ShowMaterialInformation(WorkpieceModel workpiece, IEnumerable<TimelineItemModel> timeline, IHttpService httpService)
        {
            EnsureMaterialInformationWindow(httpService);

            _infoVm!.Kind = MaterialKind.Workpiece;
            _infoVm.SlotCode = null; // 資訊視窗不顯示倉位/操作列
            _infoVm.DetailViewModel = new WorkpieceDetailViewModel(workpiece);

            FillTimeline(timeline, _infoVm);
            ShowOrActivateInformation();
        }

        // 建立 / 還原視窗
        private void EnsureMaterialInformationWindow(IHttpService httpService)
        {
            if (_infoWindow is { IsLoaded: true }) return;

            _infoVm = new ShowMaterialWindowViewModel(httpService);
            _infoWindow = new ShowMaterialInformationWindow
            {
                DataContext = _infoVm,
                Owner = Application.Current?.MainWindow
            };

            _infoWindow.Closed += (_, __) =>
            {
                _infoWindow = null;
                _infoVm = null;
            };
        }

        // 顯示或置前
        private void ShowOrActivateInformation()
        {
            if (_infoWindow == null) return;

            if (_infoWindow.IsVisible)
            {
                _infoWindow.Activate();
                _infoWindow.Topmost = true;
                _infoWindow.Topmost = false;
                _infoWindow.Focus();
            }
            else
            {
                _infoWindow.Show();
            }
        }
    }
    // 新增：簡單的訊息型別（放在同一 namespace 下）
    public sealed class UploadSheetsClosedMessage : ValueChangedMessage<bool>
    {
        public UploadSheetsClosedMessage(bool value) : base(value) { }
    }
}
