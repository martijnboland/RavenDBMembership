using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Security;
using NJasmine;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Client;
using Raven.Database.Config;
using RavenDBMembership.Provider;

namespace RavenDBMembership.IntegrationTests
{
    public class can_use_custom_membership_providers : GivenWhenThenFixture
    {
        public override void Specify()
        {
            when("using SqlMembershipProvider", delegate
            {
                then_membership_provider_should_be<SqlMembershipProvider>();

                var connectionStringField = typeof (SqlMembershipProvider).GetField("_sqlConnectionString", BindingFlags.NonPublic);

                string connectionStringValue = (string)connectionStringField.GetValue(Membership.Provider);

                expect(() => connectionStringValue == DatabaseInitialization.GetConnectionStringFor("TestSqlMembership"));
            });

            when("using RavenDBMembershipProvider embedded in-memory", delegate
            {
                then_membership_provider_should_be<RavenDBMembershipProvider>();

                then("RavenDB store is configured to run in-memory", delegate
                {
                    Assert.That(GetMembershipDocumentStore(), Is.InstanceOf<EmbeddableDocumentStore>());

                    expect(() => GetMembershipDocumentConfiguration().RunInMemory);
                    expect(() => String.IsNullOrEmpty(GetMembershipDocumentConfiguration().DataDirectory));
                });
            });

            when("using RavenDBMembershipProvider embedded w/ munin on disk", delegate
            {
                then_membership_provider_should_be<RavenDBMembershipProvider>();

                then("RavenDB store is configured to run on disk", delegate
                {
                    Assert.That(GetMembershipDocumentStore(), Is.InstanceOf<EmbeddableDocumentStore>());

                    expect(() => !GetMembershipDocumentConfiguration().RunInMemory);
                    expect(() => !String.IsNullOrEmpty(GetMembershipDocumentConfiguration().DataDirectory));
                    expect(() =>
                        GetMembershipDocumentConfiguration().DefaultStorageTypeName.Contains(
                            "Raven.Storage.Esent.Managed"));
                    //  It would be preferable to check what database type was actually used
                });
            });

            when("using RavenDBMembershipProvider embedded w/ munin on disk", delegate
            {
                then_membership_provider_should_be<RavenDBMembershipProvider>();

                expect(() => !GetMembershipDocumentConfiguration().RunInMemory);
                expect(() => !String.IsNullOrEmpty(GetMembershipDocumentConfiguration().DataDirectory));
                expect(() =>
                    GetMembershipDocumentConfiguration().DefaultStorageTypeName.Contains(
                        "Raven.Storage.Esent.TransactionalStorage"));
                //  It would be preferable to check what database type was actually used
            });
        }

        RavenConfiguration GetMembershipDocumentConfiguration()
        {
            return (GetMembershipDocumentStore() as EmbeddableDocumentStore).Configuration;
        }

        IDocumentStore GetMembershipDocumentStore()
        {
            return (Membership.Provider as RavenDBMembershipProvider).DocumentStore;
        }

        public void then_membership_provider_should_be<T>()
        {
            then("then Membership.Provider has the expected type", delegate
            {
                Assert.That(Membership.Provider, Is.InstanceOf<T>());
            });
        }
    }
}
