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
            var databasePath = NJasmine.Extras.ZipDeployTools.GetAndCheckPathOfBinDeployedFile(@"SqlMembershipDatabase\DatabaseFile.mdf");

            then("the database can be reached", delegate
            {
                string connectionString = GetConnectionStringForMdfFile(databasePath);

                var connection = arrange(() => new SqlConnection(connectionString));

                connection.Open();
                cleanup(() => connection.Close());

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
                var newPath = Path.Combine(new FileInfo(databasePath).Directory.FullName, "TestDatabaseFile.mdf");

                File.Copy(databasePath, newPath, true);

                string createScript = GetCreateScript();

                string connectionString = GetConnectionStringForMdfFile(newPath);

                var connection = arrange(() => new SqlConnection(connectionString));
                connection.Open();
                cleanup(() => connection.Close());
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

        string GetConnectionStringForMdfFile(string databasePath)
        {
            var connectionString =
                @"Database=SqlMembership;Data Source=.\SQLEXPRESS;AttachDbFileName=$path;Integrated Security=True;User Instance=True";

            connectionString = connectionString.Replace("$path", databasePath);

            return connectionString;
        }
    }
}
