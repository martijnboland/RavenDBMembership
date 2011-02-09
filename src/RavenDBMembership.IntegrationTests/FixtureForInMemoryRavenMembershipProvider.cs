using System;
using System.Web.Security;
using Raven.Client.Client;

namespace RavenDBMembership.IntegrationTests
{
    public class FixtureForInMemoryRavenMembershipProvider : MembershipProviderFixture
    {
        public override MembershipProvider GetProvider()
        {
            var store = new EmbeddableDocumentStore()
            {
                RunInMemory = true
            };

            return new RavenDBMembershipProviderThatDisposesStore()
            {
                DocumentStore = store
            };
        }

        public class RavenDBMembershipProviderThatDisposesStore : Provider.RavenDBMembershipProvider, IDisposable
        {
            public void Dispose()
            {
                if (DocumentStore != null)
                    DocumentStore.Dispose();

                DocumentStore = null;
            }
        }
    }
}