using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages; 
using ControlzEx.Standard;
using FMSFrontend.Extensions;
using FMSFrontend.Features.Services;
using FMSFrontend.Interfaces;
using FMSFrontend.Models;
using FMSFrontend.ViewModels;
using FMSFrontend.ViewModels.Production;
using FMSFrontend.ViewModels.Windows;
using FMSFrontend.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static FMSFrontend.ViewModels.ElectrodeDetailViewModel;
namespace FMSFrontend.Services
{
    
    public class WindowService : IWindowService
    {
        private ShowMaterialWindow? _materialWindow;
        // 移除 readonly，改為可為 null 的欄位，稍後在 EnsureMaterialWindow 建立
        private ShowMaterialWindowViewModel? _vm;  // ← 單一 VM，重複使用

        //  private readonly IServiceProvider _serviceProvider;

        private readonly IServiceProvider _provider;


        public WindowService(IServiceProvider provider)
        {
            _provider = provider;
        }


        public void ShowUploadSheetWindow()
        {
            // 透過 DI 取得 Window 實例
            var window = _provider.GetRequiredService<UploadsheetsWindow>();
           
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
            // 透過 DI 取得 Window 實例
            var window = _provider.GetRequiredService<MaterialPairWindow>();
            // 初始化 ViewModel（把 targetElectrodeName 傳進去）
            if (window.DataContext is MaterialPairViewModel vm)
            {
                vm.Initialize(isElectrode);
            }
            window.ShowDialog();
        }
        public void ShowProbePairWindow()
        {
            // 透過 DI 取得 Window 實例
            var window = _provider.GetRequiredService<ProbePairWindow>();
            window.ShowDialog();
        }
        public WorksheetItem? ShowSelectWorksheetWindow(IList<WorksheetItem> items)
        {
            // 從 DI 取得 Window
            var window = _provider.GetRequiredService<SelectWorksheetWindow>();

            // vm 需先宣告
            SelectWorksheetWindowViewModel? vm = window.DataContext as SelectWorksheetWindowViewModel;

            if (vm != null)
            {
                vm.Initialize(items);
            }
            else
            {
                throw new InvalidOperationException(
                    "SelectWorksheetWindow 的 DataContext 不是 SelectWorksheetWindowViewModel");
            }

            bool? dialogResult = window.ShowDialog();

            if (dialogResult == true)
            {
                // 1) 從 Window.Tag 取回結果（如果 OnConfirm 用 Tag）
                if (window.Tag is WorksheetItem tagItem)
                    return tagItem;

                // 2) 或直接用 VM.SelectedWorksheet
                return vm.SelectedWorksheet;
            }

            return null;
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
        public void ShowMaterial(object detailViewModel, IEnumerable<TimelineItemViewModel> timeline, MaterialKind kind, string? slotCode = null)
        {
            EnsureMaterialWindow();
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

        public void ShowWorkpiece(WorkpieceModel workpiece, IEnumerable<TimelineItemModel> timeline, string? slotCode = null)
        {
            EnsureMaterialWindow();

            _vm!.Kind = MaterialKind.Workpiece;
            _vm.SlotCode = slotCode ?? (!string.IsNullOrWhiteSpace(workpiece?.No) ? workpiece.No : workpiece?.Name);
            _vm.DetailViewModel = new WorkpieceDetailViewModel(workpiece ?? new WorkpieceModel()); 
            _vm.IsLocked = workpiece?.WorkRestriction ?? false;
            _vm.IsDisabled = workpiece?.StorageRestriction ?? false;

            FillTimeline(timeline);
            ShowOrActivate();
        }

        public void ShowElectrode(ElectrodeModel electrode, IEnumerable<TimelineItemModel> timeline, string? slotCode = null)
        {
            EnsureMaterialWindow();

            _vm!.Kind = MaterialKind.Electrode;
            _vm.SlotCode = slotCode ?? (!string.IsNullOrWhiteSpace(electrode?.No) ? electrode.No : electrode?.Name);
            _vm.DetailViewModel = new ElectrodeDetailViewModel(electrode ?? new ElectrodeModel());
            _vm.IsLocked = electrode?.ElecRestriction ?? false;
            _vm.IsDisabled = electrode?.StorageRestriction ?? false;

            FillTimeline(timeline);
            ShowOrActivate();
        }

        public void ShowMaterialEmpty(Slot slot)
        {
            EnsureMaterialWindow();

            _vm!.Kind = MaterialKind.None;
            _vm.SlotCode = slot.SlotCode;
            _vm.IsDisabled = slot.StorageRestriction;
            _vm.CurrentStorageState = slot.StorageStatus; // ★新增：Booked/Vacant/Occupy
            _vm.DetailViewModel = new EmptyMaterialDetailViewModel();
            _vm.Timeline.Clear();

            ShowOrActivate();
        }

        // 保留舊版無回傳功能（若其他舊程式碼仍使用）
        public void ShowSelectSharedElectrodeWindow(string targetElectrodeName)
        {
            // 從 DI 建立 Window
            var window = _provider.GetRequiredService<SelectSharedElectrodeWindow>();

            // 從 Window 取得 ViewModel 並初始化
            if (window.DataContext is SelectSharedElectrodeViewModel vm)
                vm.Initialize(targetElectrodeName);

            // 顯示視窗
            window.ShowDialog();
        }

        // 新增：帶 out 參數取得使用者選擇結果
        public void ShowSelectSharedElectrodeWindow(string targetElectrodeName, out SelectionInfo selection)
        {
            selection = new SelectionInfo();

            // 透過 DI 取得 Window 實例
            var window = _provider.GetRequiredService<SelectSharedElectrodeWindow>();

            // 初始化 ViewModel（把 targetElectrodeName 傳進去）
            if (window.DataContext is SelectSharedElectrodeViewModel vm)
            {
                vm.Initialize(targetElectrodeName);
            }

            bool? dialogResult = window.ShowDialog();

            // 和你原本邏輯一樣，從 Tag 把結果取出來
            if (dialogResult == true && window.Tag is SharedElectrodeSelection shared)
            {
                // 對應回傳內容到 SelectionInfo
                selection.WorkOrderNo = shared.WorkOrderNo ?? string.Empty;
                selection.ElectrodeName = shared.ElectrodeName ?? string.Empty;
                selection.ElectrodeId = shared.ElectrodeId ?? string.Empty;
            }
        }

        public void ShowShutdownWindow(bool isRobotRunning)
        {
            var window = _provider.GetRequiredService<ShutdownWindow>();

            if (window.DataContext is ShutdownWindowViewModel vm)
            {
                vm.Initialize(isRobotRunning);
            }

            window.ShowDialog();
        }
        public SelectItem? ShowSelectItemWindow(SelectItemType type, IList<SelectItem> items)
        {
            // 從 DI 取得 SelectItemWindow 實例
            var window = _provider.GetRequiredService<SelectItemWindow>();

            if (window.DataContext is not SelectItemWindowViewModel vm)
                throw new InvalidOperationException("SelectItemWindow 的 DataContext 應該是 SelectItemWindowViewModel");

            // 將類型與清單注入到 ViewModel
            vm.Initialize(type, items);

            bool? dialogResult = window.ShowDialog();
            if (dialogResult == true)
            {
                // 兩種方式都支援：優先用 Tag，其次用 VM.SelectedItem
                if (window.Tag is SelectItem tagItem)
                    return tagItem;

                return vm.SelectedItem;
            }

            return null;
        }

        public string? ShowLoginWindow(List<LoginInfo> loginDatas, string initialUserName)
        {
            // 從 DI 取得 LoginWindow
            var window = _provider.GetRequiredService<LoginWindow>();

            if (window.DataContext is not LoginViewModel vm)
                throw new InvalidOperationException("LoginWindow 的 DataContext 應是 LoginViewModel");

            // 將員工清單與預設帳號帶入 VM
            vm.Initialize(loginDatas, initialUserName);

            bool? result = window.ShowDialog();
            if (result == true && !string.IsNullOrWhiteSpace(vm.Name))
            {
                return vm.Name;
            }

            return null;
        }


        private void EnsureMaterialWindow()
        {
            if (_materialWindow is { IsLoaded: true })
                return;

            _materialWindow = _provider.GetRequiredService<ShowMaterialWindow>();
            _materialWindow.Owner = Application.Current?.MainWindow;

            _vm = _materialWindow.DataContext as ShowMaterialWindowViewModel
                  ?? throw new InvalidOperationException("ShowMaterialWindow DataContext 必須是 ShowMaterialWindowViewModel");

            _materialWindow.Closed += (_, __) =>
            {
                _materialWindow = null;
                _vm = null;
            };

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
        //private static void FillTimeline(IEnumerable<TimelineItemModel> timeline, ShowMaterialWindowViewModel targetVm)
        //{
        //    if (targetVm == null) return;
        //    var dst = targetVm.Timeline; // 這應該是 ObservableCollection<TimelineItemViewModel>
        //    if (dst == null) return;

        //    dst.Clear();
        //    if (timeline == null) return;

        //    foreach (var it in timeline)
        //    {
        //        // 這裡做 Model -> ViewModel 的轉換
        //        dst.Add(new TimelineItemViewModel(it));
        //    }
        //}
        private static void FillTimeline(IEnumerable<TimelineItemModel> timeline, ObservableCollection<TimelineItemViewModel>? dst)
        {
            if (dst == null) return;
            dst.Clear();

            if (timeline == null) return;

            foreach (var it in timeline)
                dst.Add(new TimelineItemViewModel(it));
        }

        public WorkerEditResult? ShowAddWorkerWindow(List<LoginInfo> existingWorkers, string selectedNumber , string selectedName)
        {
            var window = _provider.GetRequiredService<AddWorrkerWindow>();

            if (window.DataContext is not AddWorkerViewModel vm)
                throw new InvalidOperationException("AddWorrkerWindow 的 DataContext 應該是 AddWorkerViewModel");

            vm.Initialize(existingWorkers, selectedNumber, selectedName);

            bool? result = window.ShowDialog();
            if (result == true && !string.IsNullOrWhiteSpace(vm.WorkerNumber) && !string.IsNullOrWhiteSpace(vm.WorkerName))
            {
                return new WorkerEditResult(vm.WorkerNumber, vm.WorkerName, vm.Password);
            }

            return null;
        }


        // ===== 資訊版視窗的欄位 =====
        private ShowMaterialInformationWindow? _infoWindow;
        private ShowMaterialInformationViewModel? _infoVm;

        // ===== 你要的公開 API：Electrode =====
        public void ShowMaterialInformation(ElectrodeModel electrode, IEnumerable<TimelineItemModel> timeline)
        {
            EnsureMaterialInformationWindow();

            _infoVm!.Kind = MaterialKind.Electrode;
            _infoVm.SlotCode = null; // 資訊視窗不顯示倉位/操作列
            _infoVm.DetailViewModel = new ElectrodeDetailViewModel(electrode);

            FillTimeline(timeline, _infoVm.Timeline);
            ShowOrActivateInformation();
        }

        // ===== 你要的公開 API：Workpiece（補齊介面需求） =====
        public void ShowMaterialInformation(WorkpieceModel workpiece, IEnumerable<TimelineItemModel> timeline)
        {
            EnsureMaterialInformationWindow();

            _infoVm!.Kind = MaterialKind.Workpiece;
            _infoVm.SlotCode = null; // 資訊視窗不顯示倉位/操作列
            _infoVm.DetailViewModel = new WorkpieceDetailViewModel(workpiece);

            FillTimeline(timeline, _infoVm.Timeline);
            ShowOrActivateInformation();
        }

        // 建立 / 還原視窗
        private void EnsureMaterialInformationWindow()
        {
            if (_infoWindow is { IsLoaded: true })
                return;

            _infoWindow = _provider.GetRequiredService<ShowMaterialInformationWindow>();
            _infoWindow.Owner = Application.Current?.MainWindow;

            _infoVm = _infoWindow.DataContext as ShowMaterialInformationViewModel
             ?? throw new InvalidOperationException(
                    "ShowMaterialInformationWindow DataContext 必須是 ShowMaterialInformationViewModel");

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
        public void ShowMachineWindow(MachineCardViewModel machineCard)
        {
            var window = _provider.GetRequiredService<ShowMachineWindow>();

            if (window.DataContext is not ShowMachineWindowViewModel vm)
                throw new InvalidOperationException("ShowMachineWindow 的 DataContext 應該是 ShowMachineWindowViewModel");

            vm.Initialize(machineCard);

            // 設 Owner（可選，跟你原本邏輯一樣）
            var owner = Application.Current?.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.IsActive);

            if (owner != null)
                window.Owner = owner;

            window.ShowDialog();
        }

        public void ShowRobotWindow()
        {
            var win = _provider.GetRequiredService<ShowRobotWindow>();

            if (win.DataContext is not ShowRobotViewModel vm)
                throw new InvalidOperationException("ShowRobotWindow 的 DataContext 必須是 ShowRobotViewModel");

            var owner = Application.Current?.Windows.OfType<Window>()
                .FirstOrDefault(w => w.IsActive);

            if (owner != null)
                win.Owner = owner;

            win.ShowDialog();
        }
        public ReviseProcessAction? ShowReviseWindow()
        {
            var win = _provider.GetRequiredService<ReviseProcessWindow>();

            if (win.DataContext is not ReviseProcessViewModel vm)
                throw new InvalidOperationException(
                    "ReviseProcessWindow 的 DataContext 必須是 ReviseProcessViewModel");

            var owner = Application.Current?.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.IsActive);

            if (owner != null)
                win.Owner = owner;

            win.ShowDialog();

            return vm.SelectedAction;
        }

    }
    public class SelectionInfo
    {
        public string WorkOrderId { get; set; } = "";
        public string WorkOrderNo { get; set; } = "";
        public string WorkOrderName { get; set; } = "";
        public string ElectrodeId { get; set; } = "";
        public string ElectrodeName { get; set; } = "";
        public string ElectrodeState { get; set; } = "";
    }

    // 新增：簡單的訊息型別（放在同一 namespace 下）
    public sealed class UploadSheetsClosedMessage : ValueChangedMessage<bool>
    {
        public UploadSheetsClosedMessage(bool value) : base(value) { }
    }
    public record WorkerEditResult(string Number,string Name, string Password);

}
