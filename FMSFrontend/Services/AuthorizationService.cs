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
        void WriteOperation(int No, string Note = "");
    }
    public class AuthorizationService : IAuthorizationService
    {
        private readonly UserSession _userSession;
        private readonly IWindowService _windowService;
        private readonly IOperationMessageLogService _operationService;
        private string[] MsgTableCN = Array.Empty<string>();
        private string[] MsgTableEN = Array.Empty<string>();

        public AuthorizationService(
            UserSession userSession,
            IWindowService windowService,
            IOperationMessageLogService operationService)
        {
            _userSession = userSession;
            _windowService = windowService;
            _operationService = operationService;
             ReadMsgTable();
        }
        public bool RequireLoginAndWriteOperation(int No, string Note = "")
        {
            try
            {
                if (_userSession.IsLoggedIn)
                {
                    var payload = new OperationMessageLogDto
                    {
                        TimeStamp = DateTime.Now,
                        MessageCn = MsgTableCN[No] + Note,
                        MessageEn = MsgTableEN[No] + Note,
                        SetupUser = _userSession.UserName
                    };
                    _operationService.InsertNewOperationMessageLogDataAsync(payload); // 記錄操作
                    return true;
                }
                _windowService.ShowMessage("請先登入");
                return false;
            }
            catch
            {
                _windowService.ShowMessage("寫入失敗");
                return false;
            }
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
        public void WriteOperation(int No, string Note = "")
        {
            try
            {
                var payload = new OperationMessageLogDto
                {
                    TimeStamp = DateTime.Now,
                    MessageCn = MsgTableCN[No] + Note,
                    MessageEn = MsgTableEN[No] + Note,
                    SetupUser = _userSession.UserName
                };
                _operationService.InsertNewOperationMessageLogDataAsync(payload); // 記錄操作
            }
            catch
            {
                _windowService.ShowMessage("寫入失敗");
            }
        }
        void ReadMsgTable( )
        {
            INIFile ini = new INIFile(AppDomain.CurrentDomain.BaseDirectory + "\\Basesitting.ini");
            int num = Convert.ToInt16(ini.Read("Prarm", "Language"));
            string FilePath = AppDomain.CurrentDomain.BaseDirectory + "\\Language\\OperationMessage.csv";
            //從Language//OperationMessage.csv讀取表中的 column =  num + 1的位置到 MsgTable
            if (File.Exists(FilePath))
            {
                var lines = File.ReadAllLines(FilePath, Encoding.UTF8)
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .ToArray();

                // 讀取第1欄(索引1)為中文、第2欄(索引2)為英文，若不存在則給空字串。
                var cn = new List<string>(capacity: lines.Length);
                var en = new List<string>(capacity: lines.Length);
                foreach (var line in lines)
                {
                    // 簡單以逗號切分，若有更複雜CSV需求請改用正式CSV解析器
                    var cols = line.Split(',');
                    cn.Add(cols.Length > 1 ? cols[1].Trim() : string.Empty);
                    en.Add(cols.Length > 2 ? cols[2].Trim() : string.Empty);
                }
                MsgTableCN = cn.ToArray();
                MsgTableEN = en.ToArray();
            }
            else
            {
                MsgTableCN = Array.Empty<string>();
                MsgTableEN = Array.Empty<string>();
            }
        }
    }
}
