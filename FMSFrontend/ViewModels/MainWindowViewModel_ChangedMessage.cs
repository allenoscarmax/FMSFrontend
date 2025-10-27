using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging; // ← 新增
using CommunityToolkit.Mvvm.Messaging.Messages; // ← 新增：Message 型別
using ControlzEx.Standard;
using FMSFrontend.Controls;
using FMSFrontend.Extensions;
using FMSFrontend.Helpers;
using FMSFrontend.Models;
using FMSFrontend.Services;
using FMSFrontend.ViewModels.Windows;
using FMSFrontend.Views;
using IniFile;
using OSCARMAXFMS_V3.DBmodels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json; // ← 新增：JsonElement
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls; // 放在你的 ViewModel 上方
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace FMSFrontend.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private async Task InitializeDataAsync()
        {
            try
            {
                //var t1 = FetchAsrsParametersAsync();                            
                //var t2 = FetchProductionLinesAsync();                                  // 2) ProductionLines
                //  var t3 = FetchMachinesDataAsync();                                 // 3) Machine/DB_GetAllMachines
                //  var t4 = FetchAllCommandScheduleAsync();                           // 4) CommandScheduler/GetAllCommandSchedule
                //  var t5 = FetchMachineDataAsync(0);                                 // 5) Machine/GetMachineData/0

                //  await Task.WhenAll(t1, t2, t3, t4, t5);
                //  await Task.WhenAll(t1);
            }
            catch
            {
                // 啟動期允許忽略暫時性錯誤，後續輪詢或手動刷新會再補上
            }
        }
        // 新增：封裝 ASRS 資料抓取（手臂/ASRS參數）



    }
}
