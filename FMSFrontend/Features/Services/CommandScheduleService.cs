using FMSFrontend.Features.Dtos;
using FMSFrontend.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.Features.Services
{
    public interface ICommandScheduleService
    {
        Task<List<CommandStructDto>?> GetAllCommandScheduleAsync(CancellationToken ct = default);
    }

    public class CommandScheduleService : ICommandScheduleService
    {
        private readonly IHttpService _http;
        public CommandScheduleService(IHttpService http) => _http = http;

        public async Task<List<CommandStructDto>?> GetAllCommandScheduleAsync(CancellationToken ct = default)
        {
            return await _http.GetJsonAsync<List<CommandStructDto>>(
                "CommandScheduler/GetAllCommandSchedule", ct);
        }
    }
}
