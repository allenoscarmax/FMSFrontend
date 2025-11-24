using FMSFrontend.Features.Services;
using FMSFrontend.Features.Services.FMSFrontend.Features.Services;
using FMSFrontend.Models;
using FMSFrontend.Services;
using FMSFrontend.ViewModels;
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
        void ShowMaterial(object detailViewModel, IEnumerable<TimelineItemViewModel> timeline, MaterialKind kind, IHttpService httpService, string? slotCode = null);
        void ShowWorkpiece(WorkpieceModel workpiece, IEnumerable<TimelineItemModel> timeline, IHttpService httpService, string? slotCode = null);
        void ShowElectrode(ElectrodeModel electrode, IEnumerable<TimelineItemModel> timeline, IHttpService httpService, string? slotCode = null);
        void ShowMaterialEmpty(Slot slot, IHttpService httpService);
        void ShowMaterialInformation(ElectrodeModel electrode, IEnumerable<TimelineItemModel> timeline, IHttpService httpService);
        void ShowMaterialInformation(WorkpieceModel workpiece, IEnumerable<TimelineItemModel> timeline, IHttpService httpService);

        // 保留舊版無回傳功能
        void ShowSelectSharedElectrodeWindow(string targetWorkpieceName);
        // 新增：取得選擇結果
        void ShowSelectSharedElectrodeWindow(string targetWorkpieceName, out SelectionInfo selection);
    }
}
