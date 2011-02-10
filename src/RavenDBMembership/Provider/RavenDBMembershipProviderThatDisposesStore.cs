using System;

namespace RavenDBMembership.Provider
{
    public class RavenDBMembershipProviderThatDisposesStore : Provider.RavenDBMembershipProvider, IDisposable
    {
        public RavenDBMembershipProviderThatDisposesStore(string providerName) : base(providerName)
        {
        }

        public void Dispose()
        {
            if (DocumentStore != null)
                DocumentStore.Dispose();

            DocumentStore = null;
        }
    }
}