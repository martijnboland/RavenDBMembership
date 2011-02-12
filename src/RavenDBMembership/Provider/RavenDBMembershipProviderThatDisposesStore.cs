using System;

namespace RavenDBMembership.Provider
{
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