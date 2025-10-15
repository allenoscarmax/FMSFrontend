using System;
using System.Runtime.InteropServices;
using System.Text;

namespace IniFile
{
    public class INIFile
    {
        private string filePath;
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
    

        // Constructor
        public INIFile(string filePath)
        {
            this.filePath = filePath;
        }

        // Write data to ini file
        public void Write(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, this.filePath);
        }

        // Read data value from ini file
        public string Read(string section, string key)
        {
            StringBuilder SB = new StringBuilder(256);
            int i = GetPrivateProfileString(section, key, "", SB, 256, this.filePath);
            return SB.ToString();
        }

        public string FilePath
        {
            get { return this.filePath; }
            set { this.filePath = value; }
        }
    }

}