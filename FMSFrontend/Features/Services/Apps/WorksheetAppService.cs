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
        Task<UploadWorkOrderResultDto> Upload(UploadWorkOrderRequestDto data, CancellationToken ct = default);
    }
    public class WorksheetAppService : IWorksheetAppService
    {
        private readonly IHttpService _http;
        public WorksheetAppService(IHttpService http) => _http = http;

        // ===== PUT（寫入，回傳 bool）=====
        public async Task<UploadWorkOrderResultDto?> Upload(UploadWorkOrderRequestDto data, CancellationToken ct = default)
        {
            return await _http.PutJsonAsync<UploadWorkOrderRequestDto, UploadWorkOrderResultDto>(
                "Apps/WorksheetApp/Upload",
                data,
                ct
            );
        }
    }
}
