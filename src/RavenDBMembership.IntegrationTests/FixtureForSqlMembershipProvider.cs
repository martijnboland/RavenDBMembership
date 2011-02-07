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
        public override MembershipProvider GetProvider()
        {
            string databaseName = "RavenDBMembershipTestSqlDatabase";

            string tempPath = Path.Combine(Properties.Settings.Default.AccessibleTempPath, @"RavenDBMembershipTestSqlDatabase\DatabaseFile.mdf");

            DatabaseInitialization.DetachDatabase(databaseName);
            DatabaseInitialization.RecreateDatabase("RavenDBMembershipTestSqlDatabase", tempPath);

            var result = new SqlMembershipProvider();

            NameValueCollection nameValueCollection = new NameValueCollection();

            nameValueCollection["connectionStringName"] = "StubConnectionString";

            result.Initialize(null, nameValueCollection);

            var connectionStringProperty = typeof (SqlMembershipProvider).GetField("_connectionString",
                                                                                   BindingFlags.NonPublic |
                                                                                   BindingFlags.Instance);

            Assert.That(connectionStringProperty, Is.Not.Null);

            connectionStringProperty.SetValue(result, DatabaseInitialization.GetConnectionStringFor(databaseName));

            return result;
        }
    }
}