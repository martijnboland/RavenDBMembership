using System.ComponentModel;
using System.Web.Security;
using Raven.Client.Embedded;
using RavenDBMembership.Provider;

namespace RavenDBMembership.IntegrationTests.ProviderFixtures
{
    public class OverrideForInMemoryRavenMembershipProvider : MembershipProviderOverride
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