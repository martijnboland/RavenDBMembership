using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RavenDBMembership.IntegrationTests.ProviderFixtures
{
    public class DatabaseInitialization
    {
        static public string GetConnectionStringFor(string databaseName)
        {
            return Properties.Settings.Default.SqlConnectionString.Replace("$_", databaseName);
        }

        static public void RecreateDatabase(string databaseName, string databaseMdfPath)
        {
            var databaseMdfFileInfo = new FileInfo(databaseMdfPath);
            if (!Directory.Exists(databaseMdfFileInfo.Directory.FullName))
                Directory.CreateDirectory(databaseMdfFileInfo.Directory.FullName);

            string logPath = Path.Combine(databaseMdfFileInfo.Directory.FullName,
                                          databaseMdfFileInfo.Name.ToLower().Replace(".mdf", "_log.ldf"));

            DetachDatabase(databaseName);

            ApplyAndRetryForDatabase("master", masterConnection =>
            {
                using (var createCommand = new SqlCommand(@"
USE Master
CREATE DATABASE $databaseName ON (NAME=DatabaseFile1, FILENAME= '$filename')
".Replace("$databaseName", databaseName).Replace("$filename", databaseMdfPath), masterConnection))
                {
                    createCommand.ExecuteNonQuery();
                }
            });
        }

        static public IEnumerable<string> GetCreateScript()
        {
            using (var reader = new StreamReader(
                typeof(DatabaseInitialization).Assembly.GetManifestResourceStream(
                    "RavenDBMembership.IntegrationTests.SqlMembershipDatabase.Create.sql")))
            {

                var allScripts = new List<string>();
                StringBuilder currentScript = new StringBuilder();

                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    if (Regex.Match(line, @"^\s*GO\s*$").Success)
                    {
                        allScripts.Add(currentScript.ToString());
                        currentScript = new StringBuilder();
                    }
                    else
                    {
                        currentScript.Append(line);
                        currentScript.Append("\r\n");
                    }
                }

                allScripts.Add(currentScript.ToString());

                return allScripts;
            }
        }

        static public void DetachDatabase(string databaseName)
        {
            ApplyAndRetryForDatabase("master", connection =>
            {
                string detachScript1 = @"
USE [master]
IF db_id('$databaseName') IS NOT NULL
BEGIN
    --EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = N'$databaseName'
    --EXEC sp_detach_db [$databaseName];
    
    ALTER DATABASE $databaseName SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [$databaseName]
END
".Replace("$databaseName", databaseName);

                using (var command1 = new SqlCommand(detachScript1, connection))
                {
                    command1.ExecuteNonQuery();
                }
            });
        }

        public static void ApplyAndRetryForDatabase(string databaseName, Action<SqlConnection> task)
        {
            var retriesLeft = 3;

            while(retriesLeft-- > 0)
            {
                try
                {
                    using (var connection = new SqlConnection(GetConnectionStringFor(databaseName)))
                    {
                        connection.Open();

                        try
                        {
                            task(connection);
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }
                }
                catch(SqlException e)
                {
                    // trying to be selective about what exceptions we retry on...
                    // In particular, retrying if the command failed due to a bad connection.
                    if (e.Message.Contains("transport-level"))
                        continue;

                    throw;
                }

                return;
            }
        }

        static public void RunSqlMembershipCreationScript(string databaseName)
        {
            var createScripts = GetCreateScript();

            ApplyAndRetryForDatabase(databaseName, connection =>
            {
                using (var command = new SqlCommand("PRINT('verifying SQL connection is not closed');", connection))
                {
                    command.ExecuteNonQuery();
                }
            });

            using (var connection = new SqlConnection(GetConnectionStringFor(databaseName)))
            {
                connection.Open();

                try
                {
                    foreach(var createScript in createScripts)
                    {
                        using (var command = new SqlCommand(createScript, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }
}