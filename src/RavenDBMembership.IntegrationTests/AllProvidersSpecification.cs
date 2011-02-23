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

        public sealed override void Specify()
        {
            when("using RavenMembershipProvider in-memory", delegate
            {
                var provider = new FixtureForInMemoryRavenMembershipProvider();

                arrange_provider_and_run_specification(provider);
            });

            when("using SQLMembershipProvider", delegate
            {
                var provider = new FixtureForSqlMembershipProvider();

                arrange_provider_and_run_specification(provider);
            });

            when("using raven with munin on disk", delegate
            {
                var provider = new FixtureForMuninRavenMembershipProvider();

                arrange_provider_and_run_specification(provider);
            });

            when("using raven with esent on disk", delegate
            {
                var provider = new FixtureForEsentRavenMembershipProvider();

                arrange_provider_and_run_specification(provider);
            });
        }

        private void arrange_provider_and_run_specification(MembershipProviderFixture provider)
        {
            arrange(delegate
            {
                foreach (var kvp in GetAdditionalConfiguration())
                    provider.AddConfigurationValue(kvp.Key, kvp.Value);
            });

            beforeAll(() => provider.InjectProvider());
            afterAll(() => provider.RestoreProvider());

            SpecifyForEachProvider();
        }
    }
}
