using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;

namespace sqlserverinstances
{
    internal class GetSqlServerProcesses
    {
        string mServerName = "";
        string mInstanceName = "";
        bool mIsClustered = false;
        string mVersionNumber = "";

        internal GetSqlServerProcesses()
        { }

        internal GetSqlServerProcesses(string serverName, string instanceName, bool isClustered, string versionNumber)
        {
            mServerName = serverName;
            mInstanceName = instanceName;
            mIsClustered = isClustered;
            mVersionNumber = versionNumber;
        }

        internal void GetSQLProcess()
        {
            string servicename = "MSSQL";
            string servicename2 = "SQLExpress";
            string servicename3 = "SQL Server";
            string servicename4 = "msftesql";

            string serviceoutput = string.Empty;
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController service in services)
            {
                if (service == null)
                    continue;
                if (service.ServiceName.Contains(servicename) || service.ServiceName.Contains(servicename2) || service.ServiceName.Contains(servicename3) || service.ServiceName.Contains(servicename4))
                {
                    serviceoutput = serviceoutput + System.Environment.NewLine + "Service Name = " + service.ServiceName + System.Environment.NewLine + "Display Name = " + service.DisplayName + System.Environment.NewLine + "Status = " + service.Status + System.Environment.NewLine;
                }
            }

            if (serviceoutput == "")
            {
                serviceoutput += "There are no SQL Server instances present on this machine!" + System.Environment.NewLine;
            }
           
            Console.WriteLine(serviceoutput);
        }
    }
}
