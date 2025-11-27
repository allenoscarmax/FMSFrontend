using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Dtos.Apps;
using FMSFrontend.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Services
{
    public interface IWorksheetAppService
    {
        Task<bool> Upload(UploadWorkOrderRequestDto data, CancellationToken ct = default);
    }
    public class WorksheetAppService : IWorksheetAppService
    {
        private readonly IHttpService _http;
        public WorksheetAppService(IHttpService http) => _http = http;

        // ===== PUT（寫入，回傳 bool）=====
        public async Task<bool> Upload(UploadWorkOrderRequestDto data, CancellationToken ct = default)
            => await _http.SendPutAsync("Apps/WorksheetApp/Upload", data);

    }
}
