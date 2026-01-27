using FMSFrontend.Features.Services;
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
        bool RequireLoginAndWriteOperation(string Message);
        bool RequireLogin();
    }
    public class AuthorizationService : IAuthorizationService
    {
        private readonly UserSession _userSession;
        private readonly IWindowService _windowService;
        private readonly IOperationService _operationService;

        public AuthorizationService(
            UserSession userSession,
            IWindowService windowService,
            IOperationService operationService)
        {
            _userSession = userSession;
            _windowService = windowService;
            _operationService = operationService;
        }
        public bool RequireLoginAndWriteOperation(string Message)
        {
            if (_userSession.IsLoggedIn)
            {
                _operationService.WriteAsync(Message, _userSession.UserName); // 記錄操作
                return true;
            }
            _windowService.ShowMessage("請先登入");
            return false;
        }
        public bool RequireLogin()
        {
            if (_userSession.IsLoggedIn)
            {
                return true;
            }
            _windowService.ShowMessage("請先登入");
            return false;

        }
    }
}
