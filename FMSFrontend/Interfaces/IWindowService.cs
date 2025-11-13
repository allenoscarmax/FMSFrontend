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

        void ShowMaterial(object detailViewModel, IEnumerable<TimelineItemViewModel> timeline, MaterialKind kind, IHttpService httpService,
           IElectrodeService electrodeService, IWorkpieceService workpieceService, IProbeService probeService, IStorageService storageService,
           string? slotCode = null);
        void ShowWorkpiece(WorkpieceModel workpiece, IEnumerable<TimelineItemModel> timeline, IHttpService httpService,
              IElectrodeService electrodeService, IWorkpieceService workpieceService, IProbeService probeService, IStorageService storageService,
              string? slotCode = null);

        void ShowElectrode(ElectrodeModel electrode, IEnumerable<TimelineItemModel> timeline, IHttpService httpService,
             IElectrodeService electrodeService, IWorkpieceService workpieceService, IProbeService probeService, IStorageService storageService,
             string? slotCode = null);
        // 空材料視窗
        void ShowMaterialEmpty(IHttpService httpService,
             IElectrodeService electrodeService, IWorkpieceService workpieceService, IProbeService probeService, IStorageService storageService);
        // 電極資訊視窗
        void ShowMaterialInformation(ElectrodeModel electrode, IEnumerable<TimelineItemModel> timeline, IHttpService httpService,
               IElectrodeService electrodeService, IWorkpieceService workpieceService, IProbeService probeService, IStorageService storageService);
        // 工件資訊視窗
        void ShowMaterialInformation(WorkpieceModel workpiece, IEnumerable<TimelineItemModel> timeline, IHttpService httpService,
            IElectrodeService electrodeService, IWorkpieceService workpieceService, IProbeService probeService, IStorageService storageService);


        void ShowSelectSharedElectrodeWindow(string targetWorkpieceName);
    }
}
