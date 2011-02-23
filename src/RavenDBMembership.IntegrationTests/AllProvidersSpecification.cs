using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NJasmine;
using RavenDBMembership.IntegrationTests.ProviderFixtures;

namespace RavenDBMembership.IntegrationTests
{
    public abstract class AllProvidersSpecification : GivenWhenThenFixture
    {
        public virtual IEnumerable<KeyValuePair<string, string>> GetAdditionalConfiguration()
        {
            return new Dictionary<string, string>();
        }

        public abstract void SpecifyForEachProvider();

        public override void OnFixtureCreation(object fixture)
        {
            var membershipFixture = fixture as MembershipProviderFixture;

            if (membershipFixture != null)
            {
                foreach(var kvp in GetAdditionalConfiguration())
                    membershipFixture.AddConfigurationValue(kvp.Key, kvp.Value);
            }
        }

        public sealed override void Specify()
        {
            when("using RavenMembershipProvider in-memory", delegate
            {
                importNUnit<ProviderFixtures.FixtureForInMemoryRavenMembershipProvider>();

                SpecifyForEachProvider();
            });

            when("using SQLMembershipProvider", delegate
            {
                importNUnit<ProviderFixtures.FixtureForSqlMembershipProvider>();

                SpecifyForEachProvider();
            });

            when("using raven with munin on disk", delegate
            {
                importNUnit<ProviderFixtures.FixtureForMuninRavenMembershipProvider>();

                SpecifyForEachProvider();
            });

            when("using raven with esent on disk", delegate
            {
                importNUnit<ProviderFixtures.FixtureForEsentRavenMembershipProvider>();

                SpecifyForEachProvider();
            });
        }
    }
}
