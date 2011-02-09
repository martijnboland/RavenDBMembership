using System;
using System.IO;
using System.Web.Security;
using Raven.Client.Client;
using RavenDBMembership.Provider;

namespace RavenDBMembership.IntegrationTests
{
    public class FixtureForMuninRavenMembershipProvider : MembershipProviderFixture
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

            return new RavenDBMembershipProviderThatDisposesStore()
            {
                DocumentStore = store
            };
        }
    }
}