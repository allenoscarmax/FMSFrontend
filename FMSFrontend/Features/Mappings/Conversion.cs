using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Dtos.Database;
using FMSFrontend.Models;
using System.Text.RegularExpressions;
namespace FMSFrontend.Features.Mappings
{
    public static class Conversion
    {
        private static bool JusticeShortNameFlag = false; //佑義客製需求 20260122
        public static string ShortNameConversion(bool isElectrode, string Name)
        {
            // 電極名稱規則修改為 末三碼-序號+字母 (A,B,C...)，工件名稱規則修改為 末三碼-序號
            try
            {
                if (JusticeShortNameFlag) //佑義客制規則
                {
                    var parts = Name.Split('-');
                    //顯示末三碼,不足三碼顯示全部
                    var mainNo = parts[0].Length >= 3 ? parts[0].Substring(parts[0].Length - 3) : parts[0];
                    //取得會最尾巴位文字
                    var seqNo = parts.Length > 1 ? parts[parts.Length - 1] : "";
                    int n = 0;
                    if (isElectrode)
                    {
                        if (int.TryParse(seqNo, out n))
                        {
                            return $"{mainNo}-{seqNo}{(char)('A' + int.Parse(seqNo) - 1)}";
                        }
                        else
                        {
                            return $"{mainNo}-{seqNo}";
                        }
                    }
                    else
                    {
                        return $"{mainNo}-{seqNo}";
                    }
                }

                else //一般規則
                {
                    var parts = Name.Split('-');
                    //顯示末三碼,不足三碼顯示全部
                    var mainNo = parts[0].Length >= 8 ? parts[0].Substring(parts[0].Length - 8) : parts[0];
                    //取得會最尾巴位文字
                    var seqNo = parts.Length > 1 ? parts[parts.Length - 1] : "";
                    int n = 0;
                    if (isElectrode)
                    {
                        if (int.TryParse(seqNo, out n))
                        {
                            return $"{mainNo}-{seqNo}";
                        }
                        else
                        {
                            return $"{mainNo}-{seqNo}";
                        }
                    }
                    else
                    {
                        return $"{mainNo}-{seqNo}";
                    }
                }
            }
            catch { }
            return Name;
        }
    }
}
