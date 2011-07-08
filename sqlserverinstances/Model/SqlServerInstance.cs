using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{
    public class SqlServerInstance
    {
        public string ServerName { get; set; }
        public string InstanceName { get; set; }
        public bool IsClustered { get; set; }
        public string VersionNumber { get; set; }

        public string UserName { get; set; }
        public string Password { get; set; }
        public AuthenticationType Authentication { get; set; }
    }


    public enum AuthenticationType
    {
        WindowAuthentication,
        SqlServerAuthentication
    }
}
