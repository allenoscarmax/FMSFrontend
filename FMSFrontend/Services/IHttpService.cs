using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.Services
{
    public interface IHttpService
    {
        Task SendGetAsync(string route);
        Task SendPostAsync<T>(string route, T payload);
        Task SendPutAsync<T>(string route, T payload);
        Task SendDeleteAsync(string route);
    }
}
