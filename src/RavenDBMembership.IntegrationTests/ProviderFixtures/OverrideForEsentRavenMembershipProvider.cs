using System;
using System.IO;
using System.Web.Security;
using Raven.Client.Client;
using RavenDBMembership.Provider;

namespace RavenDBMembership.IntegrationTests.ProviderFixtures
{
    public class OverrideForEsentRavenMembershipProvider : MembershipProviderOverride
    {
        public override MembershipProvider GetProvider()
        {
            string dataDirectory = Path.Combine(Properties.Settings.Default.AccessibleTempPath, "RavenDBMembershipTest.Munin");

            if (Directory.Exists(dataDirectory))
                NJasmine.Extras.DirectoryUtil.DeleteDirectory(dataDirectory);

            var store = new EmbeddableDocumentStore()
            {
                RunInMemory = false,
                DataDirectory = dataDirectory
            };
            store.Configuration.DefaultStorageTypeName = "Raven.Storage.Esent.TransactionalStorage, Raven.Storage.Esent";

            store.Initialize();

            return new RavenDBMembershipProviderThatDisposesStore()
            {
                DocumentStore = store
            };
        }
    }
}