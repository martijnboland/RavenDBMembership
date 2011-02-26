using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Security;
using NJasmine;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Client;
using Raven.Database.Config;
using RavenDBMembership.IntegrationTests.ProviderFixtures;
using RavenDBMembership.Provider;

namespace RavenDBMembership.IntegrationTests
{
    public class can_use_custom_membership_providers : MembershipSpecification
    {
        public override void Specify()
        {
            when("using SqlMembershipProvider", delegate
            {
                var membershipOverride = new OverrideForSqlMembershipProvider();

                arrange_membership_provider(membershipOverride);

                then_membership_provider_should_be<SqlMembershipProvider>();

                then("the connection string is set", delegate
                {
                    var connectionStringField = typeof(SqlMembershipProvider).GetField("_sqlConnectionString", BindingFlags.NonPublic | BindingFlags.Instance);

                    Assert.That(connectionStringField, Is.Not.Null);

                    string connectionStringValue = (string)connectionStringField.GetValue(Membership.Provider);

                    expect(() => connectionStringValue == DatabaseInitialization.GetConnectionStringFor(FixtureConstants.DatabaseName));
                });
            });

            when("using RavenDBMembershipProvider embedded in-memory", delegate
            {
                var membershipOverride = new OverrideForInMemoryRavenMembershipProvider();
                
                arrange_membership_provider(membershipOverride);
                
                then_membership_provider_should_be<RavenDBMembershipProvider>();

                then("RavenDB store is configured to run munin in memory", delegate
                {
                    Assert.That(GetMembershipDocumentStore(), Is.InstanceOf<EmbeddableDocumentStore>());

                    expect(() => GetMembershipDocumentConfiguration().RunInMemory);
                     
                    expect(() => String.IsNullOrEmpty(GetMembershipDocumentConfiguration().DataDirectory)
                        || !Directory.Exists(GetMembershipDocumentConfiguration().DataDirectory));
                });
            });

            when("using RavenDBMembershipProvider embedded w/ munin on disk", delegate
            {
                var membershipOverride = new OverrideForMuninRavenMembershipProvider();
                
                arrange_membership_provider(membershipOverride);

                then_membership_provider_should_be<RavenDBMembershipProvider>();

                then("RavenDB store is configured to use munin stored in disk", delegate
                {
                    Assert.That(GetMembershipDocumentStore(), Is.InstanceOf<EmbeddableDocumentStore>());

                    expect(() => !GetMembershipDocumentConfiguration().RunInMemory);
                    expect(() => !String.IsNullOrEmpty(GetMembershipDocumentConfiguration().DataDirectory));
                    
                    expect_embedded_TransactionalStorage_FriendlyName_is("Munin");
                });
            });

            when("using RavenDBMembershipProvider embedded w/ esent on disk", delegate
            {
                var membershipOverride = new OverrideForEsentRavenMembershipProvider();

                arrange_membership_provider(membershipOverride);

                then_membership_provider_should_be<RavenDBMembershipProvider>();

                then("RavenDB store is configured to use esent", delegate()
                {
                    expect(() => !GetMembershipDocumentConfiguration().RunInMemory);
                    expect(() => !String.IsNullOrEmpty(GetMembershipDocumentConfiguration().DataDirectory));

                    expect_embedded_TransactionalStorage_FriendlyName_is("Esent");
                });
            });
        }

        IDocumentStore GetMembershipDocumentStore()
        {
            return (Membership.Provider as RavenDBMembershipProvider).DocumentStore;
        }

        RavenConfiguration GetMembershipDocumentConfiguration()
        {
            return (GetMembershipDocumentStore() as EmbeddableDocumentStore).Configuration;
        }

        void expect_embedded_TransactionalStorage_FriendlyName_is(string value)
        {
            expect(() => value.Equals(
                (GetMembershipDocumentStore() as EmbeddableDocumentStore).DocumentDatabase.TransactionalStorage.
                    FriendlyName));
        }

        public void then_membership_provider_should_be<T>()
        {
            then("then Membership.Provider has the expected type", delegate
            {
                Assert.That(Membership.Provider, Is.InstanceOf<T>());
                Assert.That(Membership.Providers[FixtureConstants.NameOfConfiguredMembershipProvider], Is.SameAs(Membership.Provider));
            });
        }
    }
}
