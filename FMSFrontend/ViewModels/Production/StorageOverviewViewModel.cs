using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Standard;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Models;
using FMSFrontend.Services;
using FMSFrontend.ViewModels.Windows;
using MahApps.Metro.Controls;
using OSCARMAXFMS_V3.DBmodels;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using static FMSFrontend.ViewModels.ElectrodeDetailViewModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FMSFrontend.ViewModels.Production
{
    public partial class StorageOverviewViewModel : ObservableObject
    {
        private readonly StorageStore _storageStore;
        public StorageGroupModel StorageGroup => _storageStore.StorageGroup;

        private readonly ProductionLinesViewModel _parent;
        private readonly IHttpService _httpService;

        public StorageOverviewViewModel(ProductionLinesViewModel parent, IHttpService httpService, StorageStore storageStore)
        {
            _parent = parent;
            _httpService = httpService;

            _storageStore = storageStore;
        }

        [RelayCommand]
        private void ToggleExpand() //切換到 Detail頁面
        {
            _parent.ShowDetail("ES1");
        }

        [RelayCommand]
        private void OpenMaterial(Slot slot) //打開材料資訊視窗
        {
            _parent.OpenMaterial(slot);
        }
    }
    public class MaterialRef
    {
        public MaterialKind Kind { get; set; }
        public ElectrodeModel Electrode { get; set; } = new ElectrodeModel();
        public WorkpieceModel Workpiece { get; set; } = new WorkpieceModel();
        public IEnumerable<TimelineItemModel> Timeline { get; set; } = Enumerable.Empty<TimelineItemModel>();
    }
}
