using FMSFrontend.Extensions;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Services.FMSFrontend.Features.Services;
using FMSFrontend.Models;
using FMSFrontend.Services;
using FMSFrontend.ViewModels;
using FMSFrontend.ViewModels.Production;
using FMSFrontend.ViewModels.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FMSFrontend.ViewModels.ElectrodeDetailViewModel;

namespace FMSFrontend.Interfaces
{
    public interface IWindowService
    {
        void ShowUploadSheetWindow();
        void ShowMessage(string message);
        bool ShowYesNoDialog(string message);
        bool ShowMaterialTypeSelectWindow(out MaterialKind kind);
        void ShowMaterialPairWindow(bool isElectrode);
        void ShowProbePairWindow();
        void ShowMaterial(object detailViewModel, IEnumerable<TimelineItemViewModel> timeline, MaterialKind kind, string? slotCode = null);
        void ShowWorkpiece(WorkpieceModel workpiece, IEnumerable<TimelineItemModel> timeline,  string? slotCode = null);
        void ShowElectrode(ElectrodeModel electrode, IEnumerable<TimelineItemModel> timeline, string? slotCode = null);
        void ShowMaterialEmpty(Slot slot);
        void ShowMaterialInformation(ElectrodeModel electrode, IEnumerable<TimelineItemModel> timeline);
        void ShowMaterialInformation(WorkpieceModel workpiece, IEnumerable<TimelineItemModel> timeline);
        WorksheetItem? ShowSelectWorksheetWindow(IList<WorksheetItem> items);

        SelectItem? ShowSelectItemWindow(SelectItemType type, IList<SelectItem> items);

        string? ShowLoginWindow(List<LoginInfo> loginDatas, string initialUserName);

        void ShowShutdownWindow(bool isRobotRunning);
        void ShowMachineWindow(MachineCardViewModel machineCard);

        void ShowRobotWindow();

        ReviseProcessAction? ShowReviseWindow();
        WorkerEditResult? ShowAddWorkerWindow(List<LoginInfo> existingWorkers, string selectedName);

        // 保留舊版無回傳功能
        void ShowSelectSharedElectrodeWindow(string targetWorkpieceName);
        // 新增：取得選擇結果
        void ShowSelectSharedElectrodeWindow(string targetWorkpieceName, out SelectionInfo selection);
    }
}
