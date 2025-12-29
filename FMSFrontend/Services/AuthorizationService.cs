using FMSFrontend.Interfaces;
using FMSFrontend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSFrontend.Services
{
    public interface IAuthorizationService
    {
        bool RequireLogin();
    }
    public class AuthorizationService : IAuthorizationService
    {
        private readonly UserSession _userSession;
        private readonly IWindowService _windowService;

        public AuthorizationService(
            UserSession userSession,
            IWindowService windowService)
        {
            _userSession = userSession;
            _windowService = windowService;
        }

        public bool RequireLogin()
        {
            if (_userSession.IsLoggedIn)
                return true;

            _windowService.ShowMessage("請先登入");
            return false;
        }
    }
}
