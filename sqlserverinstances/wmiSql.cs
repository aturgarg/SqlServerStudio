using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using System.Data;
using System.Data.Sql;

namespace sqlserverinstances
{
    internal class wmiSql
    {
        
        Dictionary<string, string> sqlServersMap;

        internal void GetServers()
        {
            StringBuilder sb2 = new StringBuilder();
            string serversStr = "CP1002";
            string sqlServerServiceName = "SQLExpress";

            string[] servers = serversStr.Split(',');
            string path;

            StringBuilder sb = new StringBuilder(DateTime.Now + "\nThe following system(s) status was recorded:\n\n");
            bool reportNeeded = false;

            sb = new StringBuilder(DateTime.Now + "\nThe following system(s) status was recorded:\n\n");

            buildServerMap();


            if (isThreadEnabled())
            {
                foreach (string sqlServer in servers)
                {
                    if (serverFound(sqlServer))
                    {
                        sb2.Append(sqlServer + " SQL Server - FOUND\n");
                    }
                    else
                    {
                        sb2.Append(sqlServer + " SQL Server - UNVERIFIABLE\n");
                    }

                    try
                    {
                        path = @"\\" + sqlServer + @"\root\cimv2";

                        ManagementScope ms = new ManagementScope(path);

                        ms.Connect();

                        ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service WHERE Started = TRUE AND DisplayName='" + sqlServerServiceName + "'");
                        searcher.Scope = ms;

                        if (searcher != null && searcher.Get() != null)
                        {
                            foreach (ManagementObject service in searcher.Get())
                            {
                                sb.Append(sqlServer + " SQL Server service - RUNNING\n");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        sb.Append(sqlServer + " SQL Server service - UNVERIFIABLE\n");
                        reportNeeded = true;
                    }
                }
            }
        }

        private bool isThreadEnabled()
        {
            return true;
        }

        private void buildServerMap()
        {
            sqlServersMap = new Dictionary<string, string>();

            //get all available SQL Servers     
            SQLDMO.Application sqlApp = new SQLDMO.Application();
            SQLDMO.NameList sqlServers = sqlApp.ListAvailableSQLServers();

            ArrayList servs = new ArrayList();
            for (int i = 0; i < sqlServers.Count; i++)
            {
                object srv = sqlServers.Item(i + 1);

                if (srv != null)
                {
                    sqlServersMap.Add(srv.ToString(), srv.ToString());
                }
            }
        }

        private bool serverFound(string serverName)
        {
            bool found = false;

            string value = null;
            sqlServersMap.TryGetValue(serverName, out value);

            if (value != null)
            {
                found = true;
            }

            return found;
        }
    }
}
