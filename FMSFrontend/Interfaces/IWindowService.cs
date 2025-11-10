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

        void ShowMaterial(object detailViewModel,
                     IEnumerable<TimelineItemViewModel> timeline,
                     MaterialKind kind,
                     IHttpService httpService,
                     string? slotCode = null);

        void ShowElectrode(ElectrodeModel elec,
                           IEnumerable<TimelineItemModel> tl,
                           IHttpService httpService,
                           string? slotCode = null);

        void ShowWorkpiece(WorkpieceModel wp,
                           IEnumerable<TimelineItemModel> tl,
                           IHttpService httpService,
                           string? slotCode = null);

        void ShowMaterialEmpty(IHttpService httpService);

        // 新增：純資訊視窗（不顯示倉位/操作列）
        void ShowMaterialInformation(ElectrodeModel elec, IEnumerable<TimelineItemModel> tl, IHttpService httpService);
        void ShowMaterialInformation(WorkpieceModel wp, IEnumerable<TimelineItemModel> tl, IHttpService httpService);


        void ShowSelectSharedElectrodeWindow(string targetWorkpieceName);
    }
}
