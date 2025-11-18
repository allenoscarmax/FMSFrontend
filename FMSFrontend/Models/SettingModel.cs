
using System.Collections.ObjectModel;
using System.Windows.Data;
using static FMSFrontend.ViewModels.SettingsPageViewModel;

namespace FMSFrontend.Models
{
    public class SettingModel
    {
        List<string> AvailableLanguages = new List<string>();
        List<string> AvailableThemes = new List<string>();
        List<DeviceModel> deviceModels = new List<DeviceModel>();
    }

    public class DeviceModel
    {
        public string DeviceId = "Delta_IO";
        public string DeviceName = "Delta_IO";
        public string IpAddress = "192.168.21.239";
        public string Port = "10001";
        public string AssetNo = "-";
        public string Owner = "OscarMax";
    }
}



















