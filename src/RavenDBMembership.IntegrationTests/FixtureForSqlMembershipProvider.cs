using System;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Web.Security;
using NUnit.Framework;

namespace RavenDBMembership.IntegrationTests
{
    public class FixtureForSqlMembershipProvider : MembershipProviderFixture
    {
        public const string DatabaseName = "RavenDBMembershipTestSqlDatabase";

        public override MembershipProvider GetProvider()
        {
            string tempPath = Properties.Settings.Default.AccessibleTempPath;
            string databaseMdfPath = Path.Combine(tempPath, @"RavenDBMembershipTestSqlDatabase\DatabaseFile.mdf");

            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);

            DatabaseInitialization.DetachDatabase(DatabaseName);
            DatabaseInitialization.RecreateDatabase(DatabaseName, databaseMdfPath);

            var result = new SqlMembershipProvider();

            NameValueCollection nameValueCollection = new NameValueCollection();

            nameValueCollection["connectionStringName"] = "StubConnectionString";

            result.Initialize(null, nameValueCollection);

            var connectionStringProperty = typeof (SqlMembershipProvider).GetField("_sqlConnectionString",
                                                                                   BindingFlags.NonPublic |
                                                                                   BindingFlags.Instance);

            Assert.That(connectionStringProperty, Is.Not.Null);

            connectionStringProperty.SetValue(result, DatabaseInitialization.GetConnectionStringFor(DatabaseName));

            return result;
        }
    }
}