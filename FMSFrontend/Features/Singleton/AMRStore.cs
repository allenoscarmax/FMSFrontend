using CommunityToolkit.Mvvm.ComponentModel;
using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Mappings;
using FMSFrontend.Models;
using System.Windows;

namespace FMSFrontend.Features.Singleton
{
    public partial class AMRStore : ObservableObject
    {
        [ObservableProperty] private AMRModel amr = new();

        public void ApplyAmrDto(AMRDto dto)
        {
            var disp = Application.Current?.Dispatcher;
            void apply() => dto.ApplyAmrDto(Amr);

            if (disp != null && !disp.CheckAccess()) disp.Invoke(apply);
            else apply();
        }
    }
}
