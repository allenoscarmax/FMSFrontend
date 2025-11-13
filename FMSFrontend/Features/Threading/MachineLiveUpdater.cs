using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Features.Services.FMSFrontend.Features.Services;
using FMSFrontend.Features.Singleton;
using FMSFrontend.Models;
using FMSFrontend.Views.Windows;
using OSCARMAXFMS_V3.DBmodels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;
namespace FMSFrontend.Features.Threading
{
    public class MachineLiveUpdater : IDisposable
    {
        private readonly IMachinesService _svc_Machines;
      
        private readonly MachineStore _store;
        private readonly DispatcherTimer _timer;

        public string SelectTitle = "";

        public MachineLiveUpdater(IMachinesService machinesService, MachineStore store)
        {
            _svc_Machines = machinesService;
            _store = store;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += async (_, __) => await UpdateMachinesStatusAsync();
        }
        public async Task<bool> UpdateMachinesStatusAsync()
        {
            //try
            //{
                // === 第一段：更新 倉儲門 控制參數 ===
                var Dto = await _svc_Machines.GetAllMachinesAsync();
                if (Dto != null)
                    _store.ApplyMachinesDto(Dto);
                return true;
            //}
            //catch //(Exception ex)
            //{
            //    return false;
            //}
        }
        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();
        public void Dispose() => _timer.Stop();
    }

}
