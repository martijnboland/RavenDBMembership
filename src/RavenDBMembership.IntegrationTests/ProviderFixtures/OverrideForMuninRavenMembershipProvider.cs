using System.IO;
using System.Web.Security;
using Raven.Client.Embedded;
using RavenDBMembership.Provider;

namespace RavenDBMembership.IntegrationTests.ProviderFixtures
{
    public class OverrideForMuninRavenMembershipProvider : MembershipProviderOverride
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
            store.Configuration.DefaultStorageTypeName = "munin";

            store.Initialize();

            return new RavenDBMembershipProviderThatDisposesStore()
            {
                DocumentStore = store
            };
        }
    }
}