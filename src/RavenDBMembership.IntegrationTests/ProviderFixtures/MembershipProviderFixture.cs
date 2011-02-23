using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web.Configuration;
using System.Web.Security;
using NUnit.Framework;

namespace RavenDBMembership.IntegrationTests.ProviderFixtures
{
    public abstract class MembershipProviderFixture
    {
        public abstract MembershipProvider GetProvider();
        public virtual void PostInitializeUpdate(MembershipProvider provider) {}

        public void AddConfigurationValue(string name, string value)
        {
            // The name and type configuration values cannot be set this late.

            string[] expectedConfigurationValues = new[]
            {
                "connectionStringName","enablePasswordRetrieval", "enablePasswordReset", "requiresQuestionAndAnswer", "requiresUniqueEmail",
                "maxInvalidPasswordAttempts", "minRequiredPasswordLength", "minRequiredNonalphanumericCharacters", "passwordAttemptWindow=",
                "applicationName"
            };

            if (!expectedConfigurationValues.Contains(name))
            {
                throw new ArgumentException(
                    "MembershipProviderFixture was asked to configure unknown MembershipProvider setting '" +
                    (name ?? "<null") + ".");
            }
        }

        Dictionary<string, string> _additionalConfiguration = new Dictionary<string, string>();
        MembershipProvider _originalProvider;
        MembershipProvider _injectedProvider;

        [TestFixtureSetUp]
        public void InjectProvider()
        {
            if (MembershipIsInitialized)
                _originalProvider = MembershipProvider;
            else
                _originalProvider = null;

            _injectedProvider = GetProvider();

            InitializeMembershipProviderFromConfigEntry(_injectedProvider);

            PostInitializeUpdate(_injectedProvider);

            MembershipProvider = _injectedProvider;
            MembershipIsInitialized = true;

            MembershipProviders = new MembershipProviderCollection();
            MembershipProviders.Add(_injectedProvider);
            MembershipProviders.SetReadOnly();
        }

        [TestFixtureTearDown]
        public void RestoreProvider()
        {
            if (MembershipProvider == _injectedProvider)
            {
                var currentProviderDisposable = MembershipProvider as IDisposable;

                if (currentProviderDisposable != null)
                    currentProviderDisposable.Dispose();

                MembershipIsInitialized = _originalProvider != null;
                MembershipProvider = _originalProvider;
                _originalProvider = null;
            }
        }

        static bool MembershipIsInitialized
        {
            get { return (bool)MembershipInitializedField.GetValue(Membership.Provider); }
            set { MembershipInitializedField.SetValue(Membership.Provider, value); }
        }

        static MembershipProvider MembershipProvider
        {
            get { return MembershipProviderField.GetValue(null) as MembershipProvider; }
            set { MembershipProviderField.SetValue(null, value); }
        }

        static MembershipProviderCollection MembershipProviders
        {
            get { return MembershipProvidersField.GetValue(null) as MembershipProviderCollection; }
            set { MembershipProvidersField.SetValue(null, value); }
        }

        static FieldInfo MembershipInitializedField = typeof(Membership).GetField("s_Initialized",
                                                                                   BindingFlags.Static |
                                                                                   BindingFlags.NonPublic);

        static FieldInfo MembershipProviderField = typeof(Membership).GetField("s_Provider",
                                                                                BindingFlags.Static |
                                                                                BindingFlags.NonPublic);

        static FieldInfo MembershipProvidersField = typeof (Membership).GetField("s_Providers",
                                                                                 BindingFlags.Static |
                                                                                 BindingFlags.NonPublic);

        public void InitializeMembershipProviderFromConfigEntry(MembershipProvider result)
        {
            NameValueCollection nameValueCollection = null;

            MembershipSection membership = ConfigurationManager.GetSection("system.web/membership") as MembershipSection;

            foreach (ProviderSettings settings in membership.Providers)
            {
                if (settings.Name == FixtureConstants.NameOfConfiguredMembershipProvider)
                {
                    nameValueCollection = new NameValueCollection(settings.Parameters);
                    break;
                }
            }

            if (nameValueCollection == null)
            {
                throw new Exception("Configuration not found for membership provider RavenDBMembership.");
            }

            nameValueCollection["connectionStringName"] = "StubConnectionString";

            foreach(var kvp in _additionalConfiguration)
            {
                nameValueCollection.Add(kvp.Key, kvp.Value);
            }

            result.Initialize(FixtureConstants.NameOfConfiguredMembershipProvider, nameValueCollection);
        }
    }
}