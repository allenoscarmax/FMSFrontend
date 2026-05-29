using FMSFrontend.Features.Dtos;
using FMSFrontend.Features.Dtos.Database;
using FMSFrontend.Models;
using System.Text.RegularExpressions;
namespace FMSFrontend.Features.Mappings
{
    public static class Conversion
    {
        private static bool JusticeShortNameFlag = true; //佑義客製需求 20260122
        public static string ShortNameConversion(bool isElectrode, string Name)
        {
            // 電極名稱規則修改為 末三碼-序號+字母 (A,B,C...)，工件名稱規則修改為 末三碼-序號
            try
            {
                int showlen = 3;
                string mainNo = "";
                if (JusticeShortNameFlag) //佑義客制規則
                {
                    showlen = 3;
                    var parts = Name.Split('-');
                    if (parts.Length < 2)
                    {
                        //顯示末六碼,不足六碼顯示全部
                        return Name.Length >= 6 ? Name.Substring(Name.Length - 6) : Name;
                    }
                    else
                    {
                        if (isElectrode)
                        {
                            if (parts.Length >= 3)
                            {
                                //顯示末8碼,不足8碼顯示全部
                                for (int i = 0; i < parts.Length - 2; i++)
                                    mainNo += parts[i] + (parts.Length - 2 != i + 1 ? "-" : "");
                                int lastHyphenIndex = mainNo.LastIndexOf('_');
                                if (lastHyphenIndex != -1) mainNo = mainNo.Substring(0, lastHyphenIndex);

                                mainNo = mainNo.Length >= showlen ? mainNo.Substring(mainNo.Length - showlen) : mainNo;
                            }
                            else
                            {
                                //顯示末8碼,不足8碼顯示全部
                                mainNo = parts[0].Length >= showlen ? parts[0].Substring(parts[0].Length - showlen) : parts[0];
                            }
                        }
                        else
                        {
                            for (int i = 0; i < parts.Length - 1; i++)
                                mainNo += parts[i] + (parts.Length - 1 != i + 1 ? "-" : "");
                            int lastHyphenIndex = mainNo.LastIndexOf('_');
                            if (lastHyphenIndex != -1) mainNo = mainNo.Substring(0, lastHyphenIndex);
                            mainNo = mainNo.Length >= showlen ? mainNo.Substring(mainNo.Length - showlen) : mainNo;
                        }
                    }
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
                    showlen = 8;
                    var parts = Name.Split('-');
                    if (parts.Length < 2)
                    {
                        //顯示末六碼,不足六碼顯示全部
                        return Name.Length >= 6 ? Name.Substring(Name.Length - 6) : Name;
                    }
                    else
                    {
                        if (isElectrode)
                        {
                            if (parts.Length >= 3)
                            {
                                //顯示末8碼,不足8碼顯示全部
                                for (int i = 0; i < parts.Length - 2; i++)
                                    mainNo += parts[i] + (parts.Length - 2 != i + 1 ? "-" : "");
                                mainNo = mainNo.Length >= showlen ? mainNo.Substring(mainNo.Length - showlen) : mainNo;
                            }
                            else
                            {
                                //顯示末8碼,不足8碼顯示全部
                                mainNo = parts[0].Length >= showlen ? parts[0].Substring(parts[0].Length - showlen) : parts[0];
                            }
                        }
                        else
                        {
                            for (int i = 0; i < parts.Length - 1; i++)
                                mainNo += parts[i] + (parts.Length - 1 != i + 1 ? "-" : "");
                            mainNo = mainNo.Length >= showlen ? mainNo.Substring(mainNo.Length - showlen) : mainNo;
                        }
                    }
                    //取得會最尾巴位文字
                    var seqNo = parts.Length > 1 ? parts[parts.Length - 1] : "";
                    return $"{mainNo}-{seqNo}";
                }
            }
            catch { }
            return Name;
        }
    }
}
