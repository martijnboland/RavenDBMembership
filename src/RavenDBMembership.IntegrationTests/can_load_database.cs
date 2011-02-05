using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using NJasmine;
using NUnit.Framework;

namespace RavenDBMembership.IntegrationTests
{
    public class can_load_database : GivenWhenThenFixture
    {
        public override void Specify()
        {

            var databasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                            @"SqlMembershipDatabase\DatabaseFile.mdf");

            var secondPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SqlMembershipDatabase\Lol.mdf");

            arrange(delegate
            {
                if (Directory.Exists(databasePath))
                {
                    Directory.Delete(databasePath, true);
                }

                if (Directory.Exists(secondPath))
                {
                    Directory.Delete(secondPath, true);
                }
            });

            then("the database can be reached", delegate
            {
                string connectionString = GetConnectionStringForMdfFile("TestDatabase", databasePath);

                var connection = arrange(() => new SqlConnection(connectionString));

                connection.Open();
                cleanup(delegate
                {
                    connection.Close();
                    DetachDatabase("TestDatabase", connectionString);
                });

                var command = arrange(() => new SqlCommand("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'", connection));

                command.ExecuteNonQuery();
            });

            then("the database scripts can be loaded", delegate
            {
                string createScript = GetCreateScript();

                Assert.That(createScript, Is.StringContaining("TABLE"));
            });

            then("the database can be generated", delegate
            {
                File.Copy(databasePath, secondPath, true);

                string createScript = GetCreateScript();

                string connectionString = GetConnectionStringForMdfFile("SqlMembership", secondPath);

                var connection = arrange(() => new SqlConnection(connectionString));
                connection.Open();
                cleanup(delegate
                {
                    Console.WriteLine("closing second connection");
                    connection.Close();
                });
                var command = arrange(() => new SqlCommand(createScript, connection));

                command.ExecuteNonQuery();
            });
        }

        string GetCreateScript()
        {
            return new StreamReader(
                this.GetType().Assembly.GetManifestResourceStream(
                    "RavenDBMembership.IntegrationTests.SqlMembershipDatabase.Create.sql")).ReadToEnd();
        }

        string GetConnectionStringForMdfFile(string databaseName, string databasePath)
        {
            var connectionString =
                @"Database=$name;Data Source=.\SQLEXPRESS;AttachDbFileName=$path;Integrated Security=True;User Instance=True";

            connectionString = connectionString.Replace("$name", databaseName).Replace("$path", databasePath);

            Console.WriteLine("Using connection string: " + databasePath);

            return connectionString;
        }

        public void DetachDatabase(string databaseName, string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                try
                {
                    using (var command = new SqlCommand(@"
use [master]
ALTER DATABASE $databaseName SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
".Replace("$databaseName", databaseName), connection))
                    {
                        command.ExecuteNonQuery();
                    } 
                    
                    using (var command = new SqlCommand(@"
EXEC sp_detach_db $databaseName;
".Replace("$databaseName", databaseName), connection))
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
    }
}
