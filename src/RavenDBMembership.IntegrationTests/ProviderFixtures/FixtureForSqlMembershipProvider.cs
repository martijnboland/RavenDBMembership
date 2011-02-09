using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Security;
using NUnit.Framework;

namespace RavenDBMembership.IntegrationTests.ProviderFixtures
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

            // Try to load the configuration values in the config file for this
            // membership provider
            NameValueCollection nameValueCollection = new NameValueCollection(
                Membership.Providers.AsQueryable().OfType<ProviderSettings>().Where(p => p.Name == "RavenDBMembership").
                    Single().Parameters);

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