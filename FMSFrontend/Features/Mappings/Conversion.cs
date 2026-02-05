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
                    if (isElectrode)
                        return Regex.Match(Name, @"_(\d+-[A-Za-z0-9]+)").Groups[1].Value;
                    else
                    {
                        //return Regex.Match(Name, @"-(\d+_\d+-[A-Za-z]+)$").Groups[1].Value;
                        //改成顯示最後六碼
                        return Name.Length >= 6 ? Name.Substring(Name.Length - 6) : Name;
                    }

                }
            }
            catch { }
            return Name;
        }
    }
}
