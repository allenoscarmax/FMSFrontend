using System;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CSV
{
    public class CSV
    {
        /*
        public static DataTable TxtConvertToDataTable(string Fil, string TableName, string delimiter, int[] D_Type)
        {
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            StreamReader s = new StreamReader(Fil, System.Text.Encoding.Default);
            //string ss = s.ReadLine();//skip the first line
            string[] columns = s.ReadLine().Split(delimiter.ToCharArray());
            ds.Tables.Add(TableName);
            foreach (string col in columns)
            {
                bool added = false;
                string next = "";
                int i = 0;
                while (!added)
                {
                    string columnname = col + next;
                    //columnname = columnname.Replace("#", "");
                    //columnname = columnname.Replace("'", "");
                    //columnname = columnname.Replace("&", "");

                    if (!ds.Tables[TableName].Columns.Contains(columnname))
                    {
                        ds.Tables[TableName].Columns.Add(columnname);
                        added = true;
                    }
                    else
                    {
                        i++;
                        next = "_" + i.ToString();
                    }
                }
            }

            if (D_Type != null)
            {
                for (int i = 0; i < D_Type.Length; i++)
                {
                    switch (D_Type[i])
                    {
                        case 0:
                            ds.Tables[TableName].Columns[i].DataType = typeof(string);
                            break;
                        case 1:
                            ds.Tables[TableName].Columns[i].DataType = typeof(int);
                            break;
                        case 2:
                            ds.Tables[TableName].Columns[i].DataType = typeof(double);
                            break;
                    }

                }
            }



            string AllData = s.ReadToEnd();
            string[] rows = OSCARMAX_Data.Txt_Split(AllData);

            for (int i = 0; i < rows.Length; i++)
            {
                if (string.IsNullOrEmpty(rows[i]) != true)
                {
                    string[] items = rows[i].Split(delimiter.ToCharArray());
                    int Check = 0;
                    if (D_Type != null)
                    {
                        for (int j = 0; j < D_Type.Length; j++)
                        {
                            if (D_Type[j] == 1 || D_Type[j] == 2)
                            {
                                if (string.IsNullOrEmpty(items[j]) == true)
                                {
                                    items[j] = "0";
                                    Check = -1;
                                }

                            }
                        }
                    }
                    if (Check == 0)
                    {
                        ds.Tables[TableName].Rows.Add(items);
                    }

                }

            }

            s.Close();

            dt = ds.Tables[0];

            return dt;
        }
        public static string[] Txt_Split(string Vaule)//字串換行分割 回傳陣列
        {
            string[] lines = Vaule.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            return lines;
        }
        */
    }

}