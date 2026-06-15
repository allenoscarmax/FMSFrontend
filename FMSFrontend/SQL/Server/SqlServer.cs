using IniFile;
using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace FMSFrontend.SQL.Server
{
    public interface ISqlServer
    {
        string ServerName { get; }
        string DatabaseName { get; set; }
        string UserName { get; }
        string Password { get; }
        string ConnectionString { get; }
        void UpdateServerIp(string ip);
        SqlConnection CreateConnection();
    }
    public class SqlServer : ISqlServer
    {
        public string ServerName { get; private set; } = @"(localdb)\FMSLocalDB";

        public string DatabaseName { get; set; } = "FMS_System_Data";

        public string UserName { get; private set; } = "sa";

        public string Password { get; private set; } = "b17589611";

        public string ConnectionString =>
            $"Server={ServerName};" +
            $"Database={DatabaseName};" +
            $"User ID={UserName};" +
            $"Password={Password};" +
            $"TrustServerCertificate=True;";

        public SqlServer()
        {
            var ini = new INIFile(AppDomain.CurrentDomain.BaseDirectory + "\\Basesitting.ini");

            var sqlServer = ini.Read("Prarm", "SqlServer");
            var databaseName = ini.Read("Prarm", "SqlDatabase");
            var sqlUser = ini.Read("Prarm", "SqlUser");
            var sqlPassword = ini.Read("Prarm", "SqlPassword");

            if (!string.IsNullOrWhiteSpace(sqlServer))
            {
                UpdateServerIp(sqlServer);
            }

            if (!string.IsNullOrWhiteSpace(databaseName))
            {
                DatabaseName = databaseName;
            }

            if (!string.IsNullOrWhiteSpace(sqlUser))
            {
                UserName = sqlUser;
            }

            if (!string.IsNullOrWhiteSpace(sqlPassword))
            {
                Password = sqlPassword;
            }
        }

        public void UpdateServerIp(string ip)
        {
            ServerName = ip;
        }

        public SqlConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }

    }
}