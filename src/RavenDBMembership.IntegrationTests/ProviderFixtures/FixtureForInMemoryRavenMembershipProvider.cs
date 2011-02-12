using System.Web.Security;
using Raven.Client.Client;
using RavenDBMembership.Provider;

namespace RavenDBMembership.IntegrationTests.ProviderFixtures
{
    public class FixtureForInMemoryRavenMembershipProvider : MembershipProviderFixture
    {
        public override MembershipProvider GetProvider()
        {
            var store = new EmbeddableDocumentStore()
            {
                RunInMemory = true
            };
            store.Initialize();

            return new RavenDBMembershipProviderThatDisposesStore()
            {
                DocumentStore = store
            };
        }
    }
}