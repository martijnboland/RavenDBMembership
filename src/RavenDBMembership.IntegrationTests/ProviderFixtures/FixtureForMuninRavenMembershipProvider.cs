using System.IO;
using System.Web.Security;
using Raven.Client.Client;
using RavenDBMembership.Provider;

namespace RavenDBMembership.IntegrationTests.ProviderFixtures
{
    public class FixtureForMuninRavenMembershipProvider : MembershipProviderFixture
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
            store.Initialize();

            return new RavenDBMembershipProviderThatDisposesStore(FixtureConstants.NameOfConfiguredMembershipProvider)
            {
                DocumentStore = store
            };
        }
    }
}