using System;
using System.Reflection;
using System.Web.Security;
using NUnit.Framework;

namespace RavenDBMembership.IntegrationTests
{
    public abstract class MembershipProviderFixture
    {
        public abstract MembershipProvider GetProvider();

        MembershipProvider _injectedProvider;

        [TestFixtureSetUp]
        public void InjectProvider()
        {
            Assert.That(MembershipIsInitialized, Is.False);
            Assert.That(MembershipProvider, Is.Null);

            MembershipProvider = _injectedProvider = GetProvider();
            MembershipIsInitialized = true;
        }

        [TestFixtureTearDown]
        public void RestoreProvider()
        {
            if (MembershipProvider == _injectedProvider)
            {
                var currentProviderDisposable = MembershipProvider as IDisposable;

                if (currentProviderDisposable != null)
                    currentProviderDisposable.Dispose();

                MembershipIsInitialized = false;
                MembershipProvider = null;
            }
        }

        static bool MembershipIsInitialized
        {
            get { return (bool)MembershipInitializedField.GetValue(Membership.Provider); }
            set { MembershipProviderField.SetValue(Membership.Provider, value); }
        }

        static MembershipProvider MembershipProvider
        {
            get { return MembershipProviderField.GetValue(Membership.Provider) as MembershipProvider; }
            set { MembershipProviderField.SetValue(Membership.Provider, value); }
        }

        static FieldInfo MembershipInitializedField = typeof(Membership).GetField("s_Initialized",
                                                                                   BindingFlags.Static |
                                                                                   BindingFlags.NonPublic);

        static FieldInfo MembershipProviderField = typeof(Membership).GetField("s_Provider",
                                                                                BindingFlags.Static |
                                                                                BindingFlags.NonPublic);

    }
}