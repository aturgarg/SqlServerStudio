using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.IO;
using Utility;
using Model;
using System.Data.SqlClient;

namespace sqlserverinstances
{
    class Program
    {
        static void Main(string[] args)
        {
            Program obj = new Program();
            //obj.GetSqlInstances();

            //wmiSql objWMI = new wmiSql();
            //objWMI.GetServers();

            List<SqlServerInstance> sqlList = obj.AllSqlInstances();

            Console.ReadLine();
        }

        public void GetSqlInstances()
        {
            AllSqlInstances();
        }

        private List<SqlServerInstance> AllSqlInstances()
        {
            List<SqlServerInstance> sqlInstanceList = new List<SqlServerInstance>();

            SqlDataSourceEnumerator SqlEnumerator;
            SqlEnumerator = SqlDataSourceEnumerator.Instance;

            DataTable dTable = SqlEnumerator.GetDataSources();

            // Display the contents of the table.
            //DisplayData(dTable);

            List<DataRow> successConnection = new List<DataRow>();
            bool tcpStatus = false;
            //sqlDatSources
            if (dTable != null && dTable.Rows.Count > 0)
            {
                foreach (DataRow row in dTable.Rows)
                {
                    Console.WriteLine("Item array count " + row.ItemArray.Count().ToString());
                    //Console.WriteLine("Host name => " + row.ItemArray[0] + " :: instance name => " + row.ItemArray[1] + " :: bool => " + row.ItemArray[2] + " :: version => " + row.ItemArray[3]);
                    Console.WriteLine("Host name => " + row["ServerName"] + " :: instance name => " + row["InstanceName"] + " :: bool => " + row["IsClustered"] + " :: version => " + row["Version"]);

                    SqlServerInstance sqlInst = new SqlServerInstance();

                    sqlInst.ServerName = row["ServerName"].ToString();
                    sqlInst.InstanceName = row["InstanceName"].ToString();
                    sqlInst.IsClustered = false;
                    sqlInst.VersionNumber = row["Version"].ToString();

                    sqlInstanceList.Add(sqlInst);

                    //GetSqlServerProcesses objSqlProc = new GetSqlServerProcesses(row["ServerName"].ToString(), row["InstanceName"].ToString(), false, row["Version"].ToString());
                    //objSqlProc.GetSQLProcess();

                    tcpStatus = false;

                    /*
                    SqlTCPConnection objSqlTcp = new SqlTCPConnection();
                    //objSqlTcp.Connect(row["ServerName"].ToString(), "test");
                    tcpStatus = objSqlTcp.IsConnected(row["ServerName"].ToString(), "test");
                    if (tcpStatus == true)
                    {
                        successConnection.Add(row);
                    }
                     * */

                    Console.WriteLine("");
                }
            }

            /*
            if (successConnection.Count > 0)
            {
                foreach (DataRow rw in successConnection)
                {
                    Console.Write(rw["ServerName"] + " :: ");
                }
            }
             * */

            return sqlInstanceList;
        }

        private void DisplayData(System.Data.DataTable table)
        {
            foreach (System.Data.DataRow row in table.Rows)
            {
                foreach (System.Data.DataColumn col in table.Columns)
                {
                    Console.WriteLine("{0} = {1}", col.ColumnName, row[col]);
                }
                Console.WriteLine("============================");
            }
        }


        private static void SqlServerConnectionMethod(string[] args)
        {
            Utility.Arguments arguments = new Arguments(args);
            try
            {
                if (arguments["?"] != null)
                {
                    PrintHelp();
                    return;
                }

                string connStr = arguments["con"];
                string outputDirectory = arguments["outDir"];
                bool scriptData = arguments["d"] != null;
                bool verbose = arguments["v"] != null;
                bool scriptProperties = arguments["p"] != null;
                bool Purge = arguments["Purge"] != null;
                bool scriptAllDatabases = arguments["ScriptAllDatabases"] != null;

                if (connStr == null || outputDirectory == null)
                {
                    PrintHelp();
                    return;
                }
                string database = null;
                using (SqlConnection sc = new SqlConnection(connStr))
                {
                    database = sc.Database;
                }

                // Purge at the Server level only when we're doing all databases
                if (Purge && scriptAllDatabases && Directory.Exists(outputDirectory))
                    PurgeDirectory(outputDirectory, "*.sql");

                if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);

                DatabaseScripter ds = new DatabaseScripter();

                if (arguments["table"] != null)
                    ds.TableFilter = arguments["table"].Split(',');
                if (arguments["view"] != null)
                    ds.ViewsFilter = arguments["view"].Split(',');
                if (arguments["sp"] != null)
                    ds.SprocsFilter = arguments["sp"].Split(',');
                if (arguments["TableOneFile"] != null)
                    ds.TableOneFile = true;
                if (arguments["ScriptAsCreate"] != null)
                    ds.ScriptAsCreate = true;
                if (arguments["Permissions"] != null)
                    ds.Permissions = true;
                if (arguments["NoCollation"] != null)
                    ds.NoCollation = true;
                if (arguments["IncludeDatabase"] != null)
                    ds.IncludeDatabase = true;
                if (arguments["CreateOnly"] != null)
                    ds.CreateOnly = true;
                if (arguments["filename"] != null)
                    ds.OutputFileName = arguments["filename"];
                ds.GenerateScripts(connStr, outputDirectory, scriptAllDatabases, Purge, scriptData, verbose, scriptProperties);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception caught in Main()");
                Console.WriteLine("---------------------------------------");
                while (e != null)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine();
                    Console.WriteLine(e.GetType().FullName);
                    Console.WriteLine();
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine("---------------------------------------");
                    e = e.InnerException;
                }
            }
        }

        public static void PurgeDirectory(string DirName, string FileSpec)
        {
            string FullPath = Path.GetFullPath(DirName);
            try
            {
                //s = string.Format(@"/c rmdir ""{0}"" /s /q", Path.GetFullPath(outputDirectory));
                //System.Diagnostics.Process.Start(@"c:\windows\system32\cmd.exe", s);

                // Remove flags from all files in the current directory
                foreach (string s in Directory.GetFiles(FullPath, FileSpec, SearchOption.AllDirectories))
                {
                    // skip files inside .svn folders (although these might be skipped regardless
                    // since they have a hidden attribute) 
                    if (!s.Contains(@"\.svn\"))
                    {
                        FileInfo file = new FileInfo(s);
                        file.Attributes = FileAttributes.Normal;
                        file.Delete();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception {0} : {1}", e.Message, FullPath);
            };
        }


        private static void PrintHelp()
        {
            Console.WriteLine(
@"ScriptDb.exe usage:

ScriptDb.exe 
    ConnectionString 
    OutputDirectory
    [-d]
    [-v]
    [-p]
    [-table:table1,table2] [-TableOneFile] 
    [-view:view1,view2] 
    [-sp:sp1,sp2] 
    [-ScriptAsCreate] 
    [-ScriptAllDatabases]
    [-Permissions] 
    [-NoCollation]
    [-CreateOnly]
    [-Purge]
    [-filename:<FileName> | -]

-con:<ConnectionString> is a connection string to the db.
-outDir:<OutputDirectory> is where the output scripts are placed.
-d script data to files for importing with bcp
-v for verbose output.
-p to script extended properties for each object.
-table - comma separated list of tables to script
-TableOneFile - script table definition into one file instad of multiple
-view - comma separated list of views to script
-sp - comma separated list of stored procedures to script
-ScriptAsCreate - script stored procedures as CREATE instead ALTER
-ScriptAllDatabases - script all databases on the current server
-IncludeDatabase - Include Database Context in scripted objects
-CreateOnly - Do not generate DROP statements
-Purge - ensures output folder is emptied of all files before generating scripts
-filename - specify output filename. If file exists, script will be appended to the end of the file
           specify '-' to output to console

Example: 

ScriptDb.exe -con:server=(local);database=pubs;trusted_connection=yes -outDir:scripts [-d] [-v] [-p] [-table:table1,table2] [-TableOneFile] [-view:view1,view2] [-sp:sp1,sp2] [-ScriptAsCreate] [-Permissions] [-NoCollation] [-IncludeDatabase] -filename:-

");
        }

    }
}
