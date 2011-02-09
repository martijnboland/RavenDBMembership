using System;
using System.IO;
using System.Web.Security;
using Raven.Client.Client;
using Raven.Client.Document;
using Raven.Database.Config;
using RavenDBMembership.Provider;

namespace RavenDBMembership.IntegrationTests
{
    public class FixtureForEsentRavenMembershipProvider : MembershipProviderFixture
    {
        public override MembershipProvider GetProvider()
        {
            string dataDirectory = Path.Combine(Properties.Settings.Default.AccessibleTempPath, "RavenDBMembershipTest.Munin");

            if (Directory.Exists(dataDirectory))
                Directory.Delete(dataDirectory);

            var store = new EmbeddableDocumentStore()
            {
                RunInMemory = false,
                DataDirectory = dataDirectory
            };
            store.Configuration.DefaultStorageTypeName = "Raven.Storage.Esent.TransactionalStorage, Raven.Storage.Esent";

            return new RavenDBMembershipProviderThatDisposesStore()
            {
                DocumentStore = store
            };
        }
    }
}