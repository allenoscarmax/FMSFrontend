using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Services;
using FMSFrontend.Interfaces;
using FMSFrontend.Models;
using IniFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FMSFrontend.Services
{
    public interface IAuthorizationService
    {
        bool RequireLoginAndWriteOperation(int No, string Note = "");
        bool RequireLogin();
        void WriteOperation(int No);
    }
    public class AuthorizationService : IAuthorizationService
    {
        private readonly UserSession _userSession;
        private readonly IWindowService _windowService;
        private readonly IOperationMessageLogService _operationService;
        private readonly string[] MsgTable ;
       
        public AuthorizationService(
            UserSession userSession,
            IWindowService windowService,
            IOperationMessageLogService operationService)
        {
            _userSession = userSession;
            _windowService = windowService;
            _operationService = operationService;
            MsgTable = ReadMsgTable();
        }
        public bool RequireLoginAndWriteOperation(int No,string Note = "")
        {
            if (_userSession.IsLoggedIn)
            {
                var payload = new OperationMessageLogDto
                {
                    TimeStamp = DateTime.Now,
                    MessageCn = MsgTable[No] + Note,
                    SetupUser = _userSession.UserName
                };
                _operationService.InsertNewOperationMessageLogDataAsync(payload); // 記錄操作
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
        public void WriteOperation(int No)
        {
            var payload = new OperationMessageLogDto
            {
                TimeStamp = DateTime.Now,
                MessageCn = MsgTable[No],
                SetupUser = _userSession.UserName
            };
            _operationService.InsertNewOperationMessageLogDataAsync(payload); // 記錄操作
        }
        string[] ReadMsgTable()
        {
            INIFile ini = new INIFile(AppDomain.CurrentDomain.BaseDirectory + "\\Basesitting.ini");
            int num = Convert.ToInt16(ini.Read("Prarm", "Language"));
            string FilePath = AppDomain.CurrentDomain.BaseDirectory + "//Language//OperationMessage.csv";
            //從Language//OperationMessage.csv讀取表中的 column =  num + 1的位置到 MsgTable
            if (File.Exists(FilePath))
            {
                var lines = File.ReadAllLines(FilePath, Encoding.UTF8)
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .ToArray();

                // If the first line is a header, keep it as data if needed; here we include all lines.
                var list = new List<string>(capacity: lines.Length);
                int targetIndex = num + 1; // column index to read
                foreach (var line in lines)
                {
                    // Basic CSV split by comma. If values may contain commas, replace with a proper CSV parser.
                    var cols = line.Split(',');
                    if (targetIndex >= 0 && targetIndex < cols.Length)
                    {
                        list.Add(cols[targetIndex].Trim());
                    }
                    else
                    {
                        list.Add(string.Empty);
                    }
                }
                return list.ToArray();
            }
            else
            {
                return Array.Empty<string>();
            }
        }
    }
}
