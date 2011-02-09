using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace RavenDBMembership.IntegrationTests.ProviderFixtures
{
    public class DatabaseInitialization
    {
        static public string GetConnectionStringFor(string databaseName)
        {
            string host = @".\SQLEXPRESS";
            return @"Database='" + databaseName + @"';Data Source=" + host + ";Integrated Security=True";
        }

        static public void RecreateDatabase(string databaseName, string databaseMdfPath)
        {
            var databaseMdfFileInfo = new FileInfo(databaseMdfPath);
            string logPath = Path.Combine(databaseMdfFileInfo.Directory.FullName,
                                          databaseMdfFileInfo.Name.ToLower().Replace(".mdf", "_log.ldf"));

            DetachDatabase(databaseName);

            using (var masterConnection = new SqlConnection(GetConnectionStringFor("master")))
            {
                masterConnection.Open();
                try
                {
                    using (var createCommand = new SqlCommand(@"
USE Master
CREATE DATABASE $databaseName ON (NAME=DatabaseFile1, FILENAME= '$filename')
".Replace("$databaseName", databaseName).Replace("$filename", databaseMdfPath), masterConnection))
                    {
                        createCommand.ExecuteNonQuery();
                    }
                }
                finally 
                {
                    masterConnection.Close();
                }
            }
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
            using (var connection = new SqlConnection(GetConnectionStringFor("master")))
            {
                connection.Open();

                try
                {
                    string detachScript = @"
USE [master]
IF db_id('$databaseName') IS NOT NULL
BEGIN
    --EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = N'$databaseName'
    --EXEC sp_detach_db [$databaseName];
    
    ALTER DATABASE $databaseName SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [$databaseName]
END
".Replace("$databaseName", databaseName);

                    using (var command = new SqlCommand(detachScript, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
            
        }

        static public void RunSqlMembershipCreationScript(string databaseName)
        {
            var createScripts = GetCreateScript();

            string connectionString = GetConnectionStringFor(databaseName);

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                try
                {
                    foreach (var createScript in createScripts)
                    {
                        try
                        {
                            using (var command = new SqlCommand(createScript, connection))
                            {
                                command.ExecuteNonQuery();
                            }
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Failing during creation script segment:");
                            Console.WriteLine(createScript);
                                
                            throw;
                        }
                    }
                }
                finally
                {
                    connection.Close();
                };
            }
        }
    }
}